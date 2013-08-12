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

        private Thread _initThread;
        private string _currentStatus;
        private int _currentPercentage;
        private OnlineParsing _parserUpdater;
        private bool _parserUpdaterWorking;
        private int _updateScanLapse;
        private DateTime _lastUpdateScan;
        private List<CParsingParameters> _parserUpdaterQueue = new List<CParsingParameters>();
        private System.Threading.Timer _scanTimer;
        private TimerCallback _timerDelegate = null;
        private Watcher _watcherUpdater;

        private List<WebScraperInputRequest> _pendingRequests;
        private Dictionary<String, CItem> _cachedItems;

        private WebScraperState _scraperState = WebScraperState.Stopped;
        private bool _importerPausedManually = false;

        public Dictionary<String, String> _pendingListSelections = new Dictionary<String, String>();
        public Dictionary<String, String> _pendingTextResults = new Dictionary<String, String>();

        private List<WebScraperItem> _scraperItems;
        private System.Timers.Timer _conflictingChecker;

        public MPTVSeriesScraper()
        {
            OnlineParsing.OnlineParsingProgress += new OnlineParsing.OnlineParsingProgressHandler(parserUpdater_OnlineParsingProgress);
            OnlineParsing.OnlineParsingCompleted += new OnlineParsing.OnlineParsingCompletedHandler(parserUpdater_OnlineParsingCompleted);

            _pendingRequests = new List<WebScraperInputRequest>();
            _cachedItems = new Dictionary<String, CItem>();

            _scraperItems = new List<WebScraperItem>();

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

            _scraperState = WebScraperState.Running;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!_importerPausedManually)
                {
                    bool conflictingRunning = ProcessUtils.IsProcessRunning(conflicting);

                    if (conflictingRunning && _scraperState != WebScraperState.Paused)
                    {
                        Log.Debug(SCRAPER_LOG_PREFIX + "Conflicting process running, pausing scraper");

                        PauseTvSeriesScraper();
                    }
                    else if (!conflictingRunning && _scraperState == WebScraperState.Paused)
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
            _parserUpdater = new OnlineParsing(this);

            DBOption.SetOptions(DBOption.cImportDelay, 5);
            int importDelay = DBOption.GetOptions(DBOption.cImportDelay);
            Log.Debug(SCRAPER_LOG_PREFIX + String.Format("Starting initial import scan in: {0} secs", importDelay));

            // Get Last Time Update Scan was run and how often it should be run
            DateTime.TryParse(DBOption.GetOptions(DBOption.cImport_OnlineUpdateScanLastTime).ToString(), out _lastUpdateScan);
            if (DBOption.GetOptions(DBOption.cImport_AutoUpdateOnlineData))
            {
                _updateScanLapse = DBOption.GetOptions(DBOption.cImport_AutoUpdateOnlineDataLapse);
            }

            // do a local scan when starting up the app if enabled - later on the watcher will monitor changes            
            if (DBOption.GetOptions(DBOption.cImport_ScanOnStartup))
            {
                // if online updates are required then this will be picked up 
                // in ImporterQueueMonitor where last update time is compared
                _parserUpdaterQueue.Add(new CParsingParameters(true, false));
            }

            // timer check every second to check for queued parsing parameters
            if (_timerDelegate == null) _timerDelegate = new TimerCallback(ImporterQueueMonitor);
            _scanTimer = new System.Threading.Timer(_timerDelegate, null, importDelay * 1000, 1000);

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

            _watcherUpdater = new Watcher(importFolders, DBOption.GetOptions(DBOption.cImport_ScanRemoteShareLapse));
            _watcherUpdater.WatcherProgress += new Watcher.WatcherProgressHandler(watcherUpdater_WatcherProgress);
        }

        private void stopFolderWatches()
        {
            _watcherUpdater.StopFolderWatch();
            _watcherUpdater.WatcherProgress -= new Watcher.WatcherProgressHandler(watcherUpdater_WatcherProgress);
            _watcherUpdater = null;
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
                        _scraperItems.Add(new WebScraperItem() { ItemId = item.m_sFullPathFileName, Title = item.m_sParsedFileName, State = "FileAdded" });
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
                lock (_parserUpdaterQueue)
                {
                    _parserUpdaterQueue.Add(new CParsingParameters(ParsingAction.List_Add, filesAdded, false, false));
                }
            }

            if (filesRemoved.Count > 0)
            {
                // queue it
                lock (_parserUpdaterQueue)
                {
                    _parserUpdaterQueue.Add(new CParsingParameters(ParsingAction.List_Remove, filesRemoved, false, false));
                }
            }
        }

        public void ImporterQueueMonitor(Object stateInfo)
        {
            if (!_parserUpdaterWorking)
            {
                // need to not be doing something yet (we don't want to accumulate parser objects !)
                bool bUpdateScanNeeded = false;

                if (_updateScanLapse > 0)
                {
                    TimeSpan tsUpdate = DateTime.Now - _lastUpdateScan;
                    if ((int)tsUpdate.TotalHours > _updateScanLapse)
                    {
                        Log.Debug(SCRAPER_LOG_PREFIX + String.Format("Online Update Scan needed, last scan run @ {0}", _lastUpdateScan));
                        _lastUpdateScan = DateTime.Now;
                        bUpdateScanNeeded = true;
                    }
                }
                if (bUpdateScanNeeded)
                {
                    // queue scan
                    lock (_parserUpdaterQueue)
                    {
                        _parserUpdaterQueue.Add(new CParsingParameters(false, bUpdateScanNeeded));
                    }
                }

                lock (_parserUpdaterQueue)
                {
                    if (_parserUpdaterQueue.Count > 0)
                    {
                        _parserUpdaterWorking = true;
                        _parserUpdater.Start(_parserUpdaterQueue[0]);
                        _parserUpdaterQueue.RemoveAt(0);
                    }
                }
            }
        }

        void parserUpdater_OnlineParsingCompleted(bool newEpisodes)
        {
            Log.Debug(SCRAPER_LOG_PREFIX + "Online parsing complete, new episodes: " + newEpisodes);
            _parserUpdaterWorking = false;

            _currentStatus = "Online parsing complete";
            _currentPercentage = 100;

            if (newEpisodes)
            {
                List<DBEpisode> eps = DBEpisode.GetMostRecent(MostRecentType.Created, 1, 100);
                foreach (DBEpisode e in eps)
                {
                    string filename = e[DBEpisode.cFilename];
                    foreach (WebScraperItem i in _scraperItems)
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

                _currentStatus = status;
                _currentPercentage = nProgress;

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
            if (_scraperState == WebScraperState.Stopped)
            {
                _conflictingChecker.Start();
                Log.Debug(SCRAPER_LOG_PREFIX + "Starting Scraper");
                InitialiseScraper();
                _scraperState = WebScraperState.Running;
                //Starting scraper importer
                _watcherUpdater.StartFolderWatch();

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
            if (_scraperState == WebScraperState.Running)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Stopping Scraper");
                _conflictingChecker.Stop();
                //Stopping scraper importer
                stopFolderWatches();
                _parserUpdater.Cancel();
                _scraperState = WebScraperState.Stopped;
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
            if (_scraperState == WebScraperState.Running)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Pausing Scraper");
                _importerPausedManually = true;
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
            _watcherUpdater.StopFolderWatch();
            _scraperState = WebScraperState.Paused;
            return true;
        }

        public WebBoolResult ResumeScraper()
        {
            if (_scraperState == WebScraperState.Paused)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Resuming Scraper");
                _importerPausedManually = false;
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
            _watcherUpdater.StartFolderWatch();
            _scraperState = WebScraperState.Running;
            return true;

        }

        public WebBoolResult TriggerUpdate()
        {
            if (_scraperState == WebScraperState.Running)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper trigger update");
                lock (_parserUpdaterQueue)
                {
                    _parserUpdaterQueue.Add(new CParsingParameters(true, false));
                }
                // Start Import if delayed
                _scanTimer.Change(1000, 1000);

                return true;
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper not running");
                return false;
            }
        }

        public WebScraperInfo GetScraperState()
        {
            return new WebScraperInfo()
            {
                CurrentAction = _currentStatus,
                CurrentProgress = _currentPercentage,
                InputNeeded = _pendingRequests.Count,
                ScraperState = _scraperState
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
                _pendingTextResults[requestId] = text;
            }
            else
            {
                _pendingListSelections.Add(requestId, matchId);

                lock (_parserUpdaterQueue)
                {
                    _parserUpdaterQueue.Add(new CParsingParameters(true, false));
                }
                // Start Import if delayed
                _scanTimer.Change(1000, 1000);
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

            if (_pendingListSelections.ContainsKey(req.Id))
            {
                CItem selectedMatch = _cachedItems[_pendingListSelections[req.Id]];
                //the user already sent a reply for this
                RemovePendingRequest(req);
                selected = selectedMatch;
                return ReturnCode.OK;
            }

            if (descriptor.m_List != null && descriptor.m_List.Count > 0)
            {
                if (descriptor.m_List[0].m_Tag == null)
                {
                    if (_pendingTextResults.ContainsKey(req.Id))
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
                        _cachedItems[match.Id] = i;

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
            foreach (WebScraperInputRequest r in _pendingRequests)
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
            foreach (WebScraperInputRequest r in _pendingRequests)
            {
                if (r.Id.Equals(req.Id))
                {
                    RemoveCachedItems(r);
                    //request already added
                    _pendingRequests.Remove(r);
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
                    _cachedItems.Remove(m.Id);
                }
            }
        }

        private void AddPendingRequest(WebScraperInputRequest req)
        {
            foreach (WebScraperInputRequest r in _pendingRequests)
            {
                if (r.Id.Equals(req.Id))
                {
                    //request already added
                    _pendingRequests.Remove(r);
                    break;
                }
            }
            _pendingRequests.Add(req);
        }

        private WebScraperInputRequest GetPendingRequest(String reqId)
        {
            foreach (WebScraperInputRequest r in _pendingRequests)
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
            return _scraperItems;
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests()
        {
            List<WebScraperItem> updated = _scraperItems.Where(x => x.InputRequest != null).ToList();
            IList<WebScraperInputRequest> inputRequests = new List<WebScraperInputRequest>();

            foreach (WebScraperItem i in updated)
            {
                inputRequests.Add(i.InputRequest);
            }

            return inputRequests;
        }


        public List<WebScraperItem> GetUpdatedScraperItems(DateTime updated)
        {
            return _scraperItems.Where(x => x.LastUpdated > updated).ToList();
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

        public WebConfigResult GetConfigSettings()
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
