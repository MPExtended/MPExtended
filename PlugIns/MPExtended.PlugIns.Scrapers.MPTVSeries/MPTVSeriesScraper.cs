using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using MPExtended.Services.ScraperService.Interfaces;
using MPExtended.Services.Common.Interfaces;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service;
using WindowPlugins.GUITVSeries;
using WindowPlugins.GUITVSeries.Feedback;

namespace MPExtended.PlugIns.Scrapers.MPTVSeries
{
    [Export(typeof(IScraperPlugin))]
    [ExportMetadata("Name", "MPTVSeries Scraper")]
    [ExportMetadata("Id", 2)]
    public class MPTVSeriesScraper : IScraperPlugin, IFeedback
    {
        private static int SCRAPER_ID = 2;
        private static String SCRAPER_NAME = "MP-TvSeries Importer";
        private static String[] conflicting = new String[] { "Configuration", "MediaPortal" };
        private static String SCRAPER_LOG_PREFIX = SCRAPER_NAME + ": ";

        class FeedBacker : IFeedback
        {
            private MPTVSeriesScraper mImporter;
            String RequestId { get; set; }
            public FeedBacker(String _requestId, MPTVSeriesScraper _importer)
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

        private Thread initThread;
        private string mCurrentStatus;
        private int mCurrentPercentage;
        private OnlineParsing m_parserUpdater;
        private bool m_parserUpdaterWorking;
        private int m_nUpdateScanLapse;
        private DateTime m_LastUpdateScan;
        private List<CParsingParameters> m_parserUpdaterQueue = new List<CParsingParameters>();
        private System.Threading.Timer m_scanTimer;
        private TimerCallback m_timerDelegate = null;
        private Watcher m_watcherUpdater;

        private List<WebScraperInputRequest> mPendingRequests;
        private Dictionary<String, CItem> mCachedItems;

        private WebScraperState mScraperState = WebScraperState.Stopped;
        private bool mImporterPausedManually = false;

        public Dictionary<String, String> mPendingListSelections = new Dictionary<String, String>();
        public Dictionary<String, String> mPendingTextResults = new Dictionary<String, String>();

        private List<WebScraperItem> scraperItems;
        private System.Timers.Timer _conflictingChecker;

        public MPTVSeriesScraper()
        {
            OnlineParsing.OnlineParsingProgress += new OnlineParsing.OnlineParsingProgressHandler(parserUpdater_OnlineParsingProgress);
            OnlineParsing.OnlineParsingCompleted += new OnlineParsing.OnlineParsingCompletedHandler(parserUpdater_OnlineParsingCompleted);

            mPendingRequests = new List<WebScraperInputRequest>();
            mCachedItems = new Dictionary<String, CItem>();

            scraperItems = new List<WebScraperItem>();

            _conflictingChecker = new System.Timers.Timer(1000);
            _conflictingChecker.AutoReset = true;
            _conflictingChecker.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

        }

        public void InitialiseScraper()
        {
            // start initialization of the moving pictures core services in a seperate thread
            #region Initialize Importer

            // Setup Importer
            InitImporter();
            #endregion

            mScraperState = WebScraperState.Running;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!mImporterPausedManually)
                {
                    bool conflictingRunning = ProcessUtils.IsProcessRunning(conflicting);

                    if (conflictingRunning && mScraperState != WebScraperState.Paused)
                    {
                        Log.Debug(SCRAPER_LOG_PREFIX + "Conflicting process running, pausing scraper");

                        PauseTvSeriesScraper();
                    }
                    else if (!conflictingRunning && mScraperState == WebScraperState.Paused)
                    {
                        Log.Debug(SCRAPER_LOG_PREFIX + "Conflicting process no longer running, resuming scraper");
                        ResumeTvSeriesScraper();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in conflicting-check", ex);
            }
        }

        private void InitImporter()
        {
            m_parserUpdater = new OnlineParsing(this);

            DBOption.SetOptions(DBOption.cImportDelay, 5);
            int importDelay = DBOption.GetOptions(DBOption.cImportDelay);
            Log.Debug(SCRAPER_LOG_PREFIX + String.Format("Starting initial import scan in: {0} secs", importDelay));

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
                        scraperItems.Add(new WebScraperItem() { ItemId = item.m_sFullPathFileName, Title = item.m_sParsedFileName, State = "FileAdded" });
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
                        Log.Debug(SCRAPER_LOG_PREFIX + String.Format("Online Update Scan needed, last scan run @ {0}", m_LastUpdateScan));
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
            Log.Debug(SCRAPER_LOG_PREFIX + "Online parsing complete, new episodes: " + newEpisodes);
            m_parserUpdaterWorking = false;

            mCurrentStatus = "Online parsing complete";
            mCurrentPercentage = 100;

            if (newEpisodes)
            {
                List<DBEpisode> eps = DBEpisode.GetMostRecent(MostRecentType.Created, 1, 100);
                foreach (DBEpisode e in eps)
                {
                    string filename = e[DBEpisode.cFilename];
                    foreach (WebScraperItem i in scraperItems)
                    {
                        if (i.ItemId == filename)
                        {
                            i.Title = e.ToString();
                            i.Description = e.onlineEpisode[DBOnlineEpisode.cEpisodeSummary];
                            i.LastUpdated = DateTime.Now;
                            i.Progress = 100;
                            i.State = "Added";
                        }
                    }
                }
            }
        }

        void parserUpdater_OnlineParsingProgress(int nProgress, ParsingProgress progress)
        {
            if (progress != null && progress.CurrentItem > 0)
            {
                String status = String.Format("progress received: {0} [{1}/{2}] {3}", progress.CurrentAction, progress.CurrentItem, progress.TotalItems, progress.CurrentProgress);
                Log.Debug(SCRAPER_LOG_PREFIX + status);

                mCurrentStatus = status;
                mCurrentPercentage = nProgress;

                if (progress.CurrentAction == ParsingAction.IdentifyNewEpisodes)
                {
                    int newEp = progress.CurrentItem;
                }
            }
        }

        public WebScraper GetScraperDescription()
        {
            return new WebScraper()
            {
                ScraperId = SCRAPER_ID,
                ScraperName = SCRAPER_NAME
            };
        }

        public WebBoolResult StartScraper()
        {
            if (mScraperState == WebScraperState.Stopped)
            {
                _conflictingChecker.Start();
                Log.Debug(SCRAPER_LOG_PREFIX + "Starting Scraper");
                InitialiseScraper();
                mScraperState = WebScraperState.Running;
                //Starting scraper importer
                m_watcherUpdater.StartFolderWatch();

                return true;
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper already running");
                return false;
            }
        }

        public WebBoolResult StopScraper()
        {
            if (mScraperState == WebScraperState.Running)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Stopping Scraper");
                _conflictingChecker.Stop();
                //Stopping scraper importer
                stopFolderWatches();
                m_parserUpdater.Cancel();
                mScraperState = WebScraperState.Stopped;
                return true;
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper not running");
                return false;
            }
        }

        public WebBoolResult PauseScraper()
        {
            if (mScraperState == WebScraperState.Running)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Pausing Scraper");
                mImporterPausedManually = true;
                return PauseTvSeriesScraper();
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper not running");
                return false;
            }
        }

        private bool PauseTvSeriesScraper()
        {
            m_watcherUpdater.StopFolderWatch();
            mScraperState = WebScraperState.Paused;
            return true;
        }

        public WebBoolResult ResumeScraper()
        {
            if (mScraperState == WebScraperState.Paused)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Resuming Scraper");
                mImporterPausedManually = false;
                return ResumeTvSeriesScraper();
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper already running");
                return false;
            }
        }

        private bool ResumeTvSeriesScraper()
        {
            m_watcherUpdater.StartFolderWatch();
            mScraperState = WebScraperState.Running;
            return true;

        }

        public WebBoolResult TriggerUpdate()
        {
            if (mScraperState == WebScraperState.Running)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper trigger update");
                lock (m_parserUpdaterQueue)
                {
                    m_parserUpdaterQueue.Add(new CParsingParameters(true, false));
                }
                // Start Import if delayed
                m_scanTimer.Change(1000, 1000);

                return true;
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper not running");
                return false;
            }
        }

        public WebScraperInfo GetScraperStatus()
        {
            return new WebScraperInfo()
            {
                CurrentAction = mCurrentStatus,
                CurrentProgress = mCurrentPercentage,
                InputNeeded = mPendingRequests.Count,
                ScraperState = mScraperState
            };
        }

        public WebBoolResult SetScraperInputRequest(String requestId, String matchId, String text)
        {
            if (text != null && !text.Equals(""))
            {
                WebScraperInputRequest req = GetPendingRequest(requestId);
                if (req != null)
                {
                    RemoveCachedItems(req);
                }

                DBOnlineSeries series = OnlineParsing.SearchForSeries(text, true, new FeedBacker(requestId, this));
                mPendingTextResults[requestId] = text;
            }
            else
            {
                mPendingListSelections.Add(requestId, matchId);

                lock (m_parserUpdaterQueue)
                {
                    m_parserUpdaterQueue.Add(new CParsingParameters(true, false));
                }
                // Start Import if delayed
                m_scanTimer.Change(1000, 1000);
            }

            return false;
        }

        public ReturnCode ChooseFromSelection(ChooseFromSelectionDescriptor descriptor, out CItem selected)
        {
            return HandleItemSelection(descriptor.m_sItemToMatch, descriptor, out selected);
        }

        private ReturnCode HandleItemSelection(string _title, ChooseFromSelectionDescriptor descriptor, out CItem selected)
        {
            WebScraperInputRequest req = new WebScraperInputRequest();
            req.Title = _title;
            req.Id = _title;

            if (mPendingListSelections.ContainsKey(req.Id))
            {
                CItem selectedMatch = mCachedItems[mPendingListSelections[req.Id]];
                //the user already sent a reply for this
                RemovePendingRequest(req);
                selected = selectedMatch;
                return ReturnCode.OK;
            }

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

                        match.Title = i.m_sName;
                        match.Description = i.m_sDescription;
                        //match.Tag = i;
                        mCachedItems[match.Id] = i;

                        if (i.m_Tag != null && i.m_Tag.GetType() == typeof(DBTable))
                        {
                            //TODO: maybe add additional info here?
                        }
                        req.InputOptions.Add(match);

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
                    RemoveCachedItems(r);
                    //request already added
                    mPendingRequests.Remove(r);
                    return;
                }
            }
        }

        private void RemoveCachedItems(WebScraperInputRequest req)
        {
            if (req.InputOptions != null)
            {
                foreach (WebScraperInputMatch m in req.InputOptions)
                {
                    mCachedItems.Remove(m.Id);
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

        private WebScraperInputRequest GetPendingRequest(String reqId)
        {
            foreach (WebScraperInputRequest r in mPendingRequests)
            {
                if (r.Id.Equals(reqId))
                {
                    return r;
                }
            }
            return null;
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

        public WebBoolResult AddItemToScraper(string title, WebMediaType type, int? provider, string itemId, int? offset)
        {
            return true;
        }


        public List<WebScraperItem> GetScraperItems()
        {
            return scraperItems;
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests()
        {
            List<WebScraperItem> updated = scraperItems.Where(x => x.InputRequest != null).ToList();
            IList<WebScraperInputRequest> inputRequests = new List<WebScraperInputRequest>();

            foreach (WebScraperItem i in updated)
            {
                inputRequests.Add(i.InputRequest);
            }

            return inputRequests;
        }


        public List<WebScraperItem> GetUpdatedScraperItems(DateTime updated)
        {
            return scraperItems.Where(x => x.LastUpdated > updated).ToList();
        }

        public WebScraperItem GetScraperItem(string itemId)
        {
            throw new NotImplementedException();
        }

        public List<WebScraperAction> GetScraperActions()
        {
            return new List<WebScraperAction>();
        }

        public WebBoolResult InvokeScraperAction(string itemId, string actionId)
        {
            return false;
        }

        public Form CreateConfig()
        {
            ConfigurationForm form = new ConfigurationForm();
            TabControl tabs = (TabControl)form.Controls[0].Controls[0].Controls[0];
            tabs.TabPages.RemoveAt(3);//follw.it
            tabs.TabPages.RemoveAt(3);//General
            tabs.TabPages.RemoveAt(3);//views/filters
            tabs.TabPages.RemoveAt(3);//layout

            return form;
        }

        public WebConfigResult GetConfig()
        {
            FileInfo path = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            return new WebConfigResult()
            {
                DllPath = path.FullName,
                PluginAssemblyName = this.GetType().FullName,
                ExternalPaths = new List<String> { path.Directory.FullName }
            };
        }
    }
}
