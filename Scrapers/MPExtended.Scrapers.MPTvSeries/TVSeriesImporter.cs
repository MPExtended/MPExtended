using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.ServiceModel;
using MPExtended.Services.ScraperService.Interfaces;
using WindowPlugins.GUITVSeries;
using WindowPlugins.GUITVSeries.Feedback;

namespace MPExtended.Scrapers.TVSeries
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class TVSeriesImporter : IScraperService, IFeedback
    {
        class FeedBacker : IFeedback
        {
            private TVSeriesImporter mImporter;
            String RequestId { get; set; }
            public FeedBacker(String _requestId, TVSeriesImporter _importer)
            {
                RequestId = _requestId;
                mImporter = _importer;
            }
            public ReturnCode ChooseFromSelection(ChooseFromSelectionDescriptor descriptor, out CItem selected)
            {
                return mImporter.HandleItemSelection(RequestId, descriptor, out selected);
            }

            public ReturnCode GetStringFromUser(GetStringFromUserDescriptor descriptor, out string input)
            {
                input = null;
                return ReturnCode.Cancel;
            }

            public ReturnCode YesNoOkDialog(ChooseFromYesNoDescriptor descriptor)
            {
                return ReturnCode.Cancel;
            }
        }

        private void CheckConflictingProgramsRunning()
        {
            bool paused = false;
            while (mImporterRunning)
            {
                if (!mImporterPausedManually)
                {
                    bool mediaportalRunning = false;
                    Process config = GetProcess("Configuration");

                    if (config != null)
                    {
                        if (config.MainWindowTitle != null)
                        {
                            mediaportalRunning = true;
                        }
                    }

                    Process mp = GetProcess("MediaPortal");
                    if (mp != null)
                    {
                        mediaportalRunning = true;
                    }

                    if (mediaportalRunning)
                    {
                        if (!paused)
                        {
                            Console.WriteLine("MediaPortal / MediaPortal config running, pausing scraper");

                            PauseTvSeriesScraper();
                            paused = true;
                        }
                    }
                    else
                    {
                        if (paused)
                        {
                            Console.WriteLine("MediaPortal / MediaPortal config no longer running, resuming scraper");
                            ResumeTvSeriesScraper();
                            paused = false;
                        }

                    }
                }

                Thread.Sleep(1000);
            }
        }

        private Thread initThread;
        private Thread mopiCheckThread;
        private bool mImporterRunning;
        private bool mImporterPaused;
        private string mCurrentStatus;
        private int mCurrentPercentage;
        private OnlineParsing m_parserUpdater;
        private bool m_parserUpdaterWorking;
        private int m_nUpdateScanLapse;
        private DateTime m_LastUpdateScan;
        private List<CParsingParameters> m_parserUpdaterQueue = new List<CParsingParameters>();
        private Timer m_scanTimer;
        private TimerCallback m_timerDelegate = null;
        private Watcher m_watcherUpdater;
        public bool mImporterPausedManually { get; set; }

        private List<WebScraperInputRequest> mPendingRequests;

        public TVSeriesImporter()
        {
            OnlineParsing.OnlineParsingProgress += new OnlineParsing.OnlineParsingProgressHandler(parserUpdater_OnlineParsingProgress);
            OnlineParsing.OnlineParsingCompleted += new OnlineParsing.OnlineParsingCompletedHandler(parserUpdater_OnlineParsingCompleted);

            mPendingRequests = new List<WebScraperInputRequest>();
        }

        public Process GetProcess(string name)
        {
            //here we're going to get a list of all running processes on
            //the computer
            foreach (Process clsProcess in Process.GetProcesses())
            {
                //now we're going to see if any of the running processes
                //match the currently running processes.
                if (clsProcess.ProcessName.Contains(name))
                {
                    //if the process is found to be running then we
                    //return a true
                    return clsProcess;
                }
            }
            //otherwise we return a false
            return null;
        }

        public void InitialiseScraper()
        {
            mImporterRunning = true;
            // start initialization of the moving pictures core services in a seperate thread
            #region Initialize Importer

            // Setup Importer
            InitImporter();
            #endregion

            mopiCheckThread = new Thread(new ThreadStart(CheckConflictingProgramsRunning));
            mopiCheckThread.Start();
        }

        private void InitImporter()
        {
            m_parserUpdater = new OnlineParsing(this);

            int importDelay = DBOption.GetOptions(DBOption.cImportDelay);
            Console.WriteLine(String.Format("Starting initial import scan in: {0} secs", importDelay));

            // Get Last Time Update Scan was run and how often it should be run
            DateTime.TryParse(DBOption.GetOptions(DBOption.cImport_OnlineUpdateScanLastTime).ToString(), out m_LastUpdateScan);
            if (DBOption.GetOptions(DBOption.cImport_AutoUpdateOnlineData))
            {
                m_nUpdateScanLapse = DBOption.GetOptions(DBOption.cImport_AutoUpdateOnlineDataLapse);
            }

            // do a local scan when starting up the app if enabled - later on the watcher will monitor changes            
            if (DBOption.GetOptions(DBOption.cImport_ScanOnStartup))
            {
                // if online updates are required then this will be picked up 
                // in ImporterQueueMonitor where last update time is compared
                m_parserUpdaterQueue.Add(new CParsingParameters(true, false));
            }

            // timer check every second to check for queued parsing parameters
            if (m_timerDelegate == null) m_timerDelegate = new TimerCallback(ImporterQueueMonitor);
            m_scanTimer = new System.Threading.Timer(m_timerDelegate, null, importDelay * 1000, 1000);

            // Setup Disk Watcher (DeviceManager) and Folder/File Watcher
            if (DBOption.GetOptions(DBOption.cImport_FolderWatch))
            {
                DeviceManager.StartMonitor();
                setUpFolderWatches();
            }
        }

        private void setUpFolderWatches()
        {
            List<String> importFolders = new List<String>();
            DBImportPath[] importPaths = DBImportPath.GetAll();
            if (importPaths != null)
            {
                // Go through all enabled import folders, and add a watchfolder on it
                foreach (DBImportPath importPath in importPaths)
                {
                    if (importPath[DBImportPath.cEnabled] != 0)
                        importFolders.Add(importPath[DBImportPath.cPath]);
                }
            }

            m_watcherUpdater = new Watcher(importFolders, DBOption.GetOptions(DBOption.cImport_ScanRemoteShareLapse));
            m_watcherUpdater.WatcherProgress += new Watcher.WatcherProgressHandler(watcherUpdater_WatcherProgress);
        }

        private void stopFolderWatches()
        {
            m_watcherUpdater.StopFolderWatch();
            m_watcherUpdater.WatcherProgress -= new Watcher.WatcherProgressHandler(watcherUpdater_WatcherProgress);
            m_watcherUpdater = null;
            DeviceManager.StopMonitor();
        }

        private void watcherUpdater_WatcherProgress(int nProgress, List<WatcherItem> modifiedFilesList)
        {
            List<PathPair> filesAdded = new List<PathPair>();
            List<PathPair> filesRemoved = new List<PathPair>();

            // go over the modified files list once in a while & update
            foreach (WatcherItem item in modifiedFilesList)
            {
                switch (item.m_type)
                {
                    case WatcherItemType.Added:
                        filesAdded.Add(new PathPair(item.m_sParsedFileName, item.m_sFullPathFileName));
                        break;

                    case WatcherItemType.Deleted:
                        filesRemoved.Add(new PathPair(item.m_sParsedFileName, item.m_sFullPathFileName));
                        break;
                }
            }

            // with out list of files, start the parsing process
            if (filesAdded.Count > 0)
            {
                // queue it
                lock (m_parserUpdaterQueue)
                {
                    m_parserUpdaterQueue.Add(new CParsingParameters(ParsingAction.List_Add, filesAdded, false, false));
                }
            }

            if (filesRemoved.Count > 0)
            {
                // queue it
                lock (m_parserUpdaterQueue)
                {
                    m_parserUpdaterQueue.Add(new CParsingParameters(ParsingAction.List_Remove, filesRemoved, false, false));
                }
            }
        }

        public void ImporterQueueMonitor(Object stateInfo)
        {
            if (!m_parserUpdaterWorking)
            {
                // need to not be doing something yet (we don't want to accumulate parser objects !)
                bool bUpdateScanNeeded = false;

                if (m_nUpdateScanLapse > 0)
                {
                    TimeSpan tsUpdate = DateTime.Now - m_LastUpdateScan;
                    if ((int)tsUpdate.TotalHours > m_nUpdateScanLapse)
                    {
                        Console.WriteLine(String.Format("Online Update Scan needed, last scan run @ {0}", m_LastUpdateScan));
                        m_LastUpdateScan = DateTime.Now;
                        bUpdateScanNeeded = true;
                    }
                }
                if (bUpdateScanNeeded)
                {
                    // queue scan
                    lock (m_parserUpdaterQueue)
                    {
                        m_parserUpdaterQueue.Add(new CParsingParameters(false, bUpdateScanNeeded));
                    }
                }

                lock (m_parserUpdaterQueue)
                {
                    if (m_parserUpdaterQueue.Count > 0)
                    {
                        m_parserUpdaterWorking = true;
                        m_parserUpdater.Start(m_parserUpdaterQueue[0]);
                        m_parserUpdaterQueue.RemoveAt(0);
                    }
                }
            }
        }

        void parserUpdater_OnlineParsingCompleted(bool newEpisodes)
        {
            Console.WriteLine("Online parsing complete: " + newEpisodes);
            m_parserUpdaterWorking = false;

            mCurrentStatus = "Online parsing complete";
            mCurrentPercentage = 100;

        }

        void parserUpdater_OnlineParsingProgress(int nProgress, ParsingProgress progress)
        {
            if (progress != null && progress.CurrentItem > 0)
            {
                String status = String.Format("progress received: {0} [{1}/{2}] {3}", progress.CurrentAction, progress.CurrentItem, progress.TotalItems, progress.CurrentProgress);
                Console.WriteLine(status);

                mCurrentStatus = status;
                mCurrentPercentage = nProgress;

                if (progress.CurrentAction == ParsingAction.IdentifyNewEpisodes)
                {
                    int newEp = progress.CurrentItem;
                }
            }
        }

        public WebResult StartScraper()
        {
            Console.WriteLine("Starting Scraper");
            InitialiseScraper();
            mImporterPaused = false;
            mImporterPausedManually = false;
            mImporterRunning = true;
            //Starting scraper importer
            m_watcherUpdater.StartFolderWatch();

            return true;
        }



        public WebResult StopScraper()
        {
            Console.WriteLine("Stopping Scraper");
            mImporterRunning = false;
            //Stopping scraper importer
            stopFolderWatches();
            m_parserUpdater.Cancel();
            return true;
        }

        public WebResult PauseScraper()
        {
            Console.WriteLine("Pausing Scraper");
            mImporterPausedManually = true;
            return PauseTvSeriesScraper();
        }

        private bool PauseTvSeriesScraper()
        {
            mImporterPaused = true;
            m_watcherUpdater.StopFolderWatch();
            return true;
        }

        public WebResult ResumeScraper()
        {
            Console.WriteLine("Resuming Scraper");
            mImporterPausedManually = false;
            return ResumeTvSeriesScraper();
        }

        private bool ResumeTvSeriesScraper()
        {
            mImporterPaused = false;
            m_watcherUpdater.StartFolderWatch();
            return true;
        }

        public WebScraperStatus GetScraperStatus()
        {
            return new WebScraperStatus() { CurrentAction = mCurrentStatus, CurrentProgress = mCurrentPercentage, InputNeeded = mPendingRequests.Count };
        }

        public WebScraperInputRequest GetScraperInputRequest(int index)
        {


            return mPendingRequests[index];
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests()
        {
            return mPendingRequests;
        }

        public WebResult SetScraperInputRequest(String requestId, String matchId, String text)
        {
            if (text != null && !text.Equals(""))
            {
                DBOnlineSeries series = OnlineParsing.SearchForSeries(text, true, new FeedBacker(requestId, this));
                mPendingTextResults[requestId] = text;
            }
            else
            {
                mPendingListSelections.Add(requestId, matchId);
            }

            return false;
        }

        public Dictionary<String, String> mPendingListSelections = new Dictionary<String, String>();
        public Dictionary<String, String> mPendingTextResults = new Dictionary<String, String>();

        public ReturnCode ChooseFromSelection(ChooseFromSelectionDescriptor descriptor, out CItem selected)
        {
            return HandleItemSelection(descriptor.m_sItemToMatch, descriptor, out selected);
        }

        private ReturnCode HandleItemSelection(string _title, ChooseFromSelectionDescriptor descriptor, out CItem selected)
        {
            WebScraperInputRequest req = new WebScraperInputRequest();
            req.Title = _title;
            req.Id = _title;

            if (descriptor.m_List != null && descriptor.m_List.Count > 0)
            {
                if (descriptor.m_List[0].m_Tag == null)
                {
                    if (mPendingTextResults.ContainsKey(req.Id))
                    {
                        selected = descriptor.m_List[0];
                        return ReturnCode.OK;
                    }
                    else
                    {
                        req.InputType = WebInputTypes.TextInput;
                        req.Text = descriptor.m_List[0].m_sName;
                    }
                }
                else
                {
                    req.InputOptions = new List<WebScraperInputMatch>();
                    req.InputType = WebInputTypes.ItemSelect;
                    foreach (CItem i in descriptor.m_List)
                    {
                        WebScraperInputMatch match = new WebScraperInputMatch();
                        match.Id = i.m_sName;
                        if (mPendingListSelections.ContainsKey(req.Id))
                        {
                            CItem selectedMatch = GetRequestMatchItem(req.Id, mPendingListSelections[req.Id]).Tag as CItem;
                            //the user already sent a reply for this
                            RemovePendingRequest(req);
                            selected = selectedMatch;
                            return ReturnCode.OK;
                        }
                        else
                        {

                            match.Title = i.m_sName;
                            match.Description = i.m_sDescription;
                            match.Tag = i;

                            if (i.m_Tag != null && i.m_Tag.GetType() == typeof(DBTable))
                            {
                                //TODO: maybe add additional info here?
                            }
                            req.InputOptions.Add(match);
                        }
                    }
                }
            }

            AddPendingRequest(req);

            selected = null;
            return ReturnCode.Cancel;
        }

        private WebScraperInputMatch GetRequestMatchItem(string _requestId, string _matchId)
        {
            foreach (WebScraperInputRequest r in mPendingRequests)
            {
                if (r.Id.Equals(_requestId))
                {
                    foreach (WebScraperInputMatch m in r.InputOptions)
                    {
                        if (m.Id.Equals(_matchId))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }

        private void RemovePendingRequest(WebScraperInputRequest req)
        {
            foreach (WebScraperInputRequest r in mPendingRequests)
            {
                if (r.Id.Equals(req.Id))
                {
                    //request already added
                    mPendingRequests.Remove(r);
                    return;
                }
            }
        }

        private void AddPendingRequest(WebScraperInputRequest req)
        {
            foreach (WebScraperInputRequest r in mPendingRequests)
            {
                if (r.Id.Equals(req.Id))
                {
                    //request already added
                    mPendingRequests.Remove(r);
                    break;
                }
            }
            mPendingRequests.Add(req);
        }

        public ReturnCode GetStringFromUser(GetStringFromUserDescriptor descriptor, out string input)
        {
            input = null;
            return ReturnCode.Cancel;
        }

        public ReturnCode YesNoOkDialog(ChooseFromYesNoDescriptor descriptor)
        {
            return ReturnCode.Cancel;
        }

        public bool IsRunning { get { return mImporterRunning; } }


    }
}
