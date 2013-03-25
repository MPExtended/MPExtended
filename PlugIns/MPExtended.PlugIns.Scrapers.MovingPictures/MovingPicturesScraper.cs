using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MPExtended.Services.ScraperService.Interfaces;
using MPExtended.Services.Common.Interfaces;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures;
using Cornerstone.GUI.Dialogs;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service;

namespace MPExtended.PlugIns.Scrapers.MovingPictures
{
    [Export(typeof(IScraperPlugin))]
    [ExportMetadata("Name", "MovingPictures Scraper")]
    [ExportMetadata("Id", 1)]
    public class MovingPicturesScraper : IScraperPlugin
    {

        private static int SCRAPER_ID = 1;
        private static String SCRAPER_NAME = "MovingPictures Importer";
        private static String SCRAPER_LOG_PREFIX = SCRAPER_NAME + ": ";
        private static String[] conflicting = new String[] { "Configuration", "MediaPortal" };



        private Thread initThread;
        private List<MovieMatch> matches = new List<MovieMatch>();
        private Thread conflictingCheckThread;
        private string mCurrentAction;
        private int mCurrentPercentage;
        private bool mImporterPausedManually;
        private WebScraperState mScraperState = WebScraperState.Stopped;

        private Dictionary<MovieMatch, WebScraperItem> scraperItems;
        private System.Timers.Timer _conflictingChecker;

        public MovingPicturesScraper()
        {
            scraperItems = new Dictionary<MovieMatch, WebScraperItem>();

            _conflictingChecker = new System.Timers.Timer(1000);
            _conflictingChecker.AutoReset = true;
            _conflictingChecker.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
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
                        Log.Info("Conflicting program running, pausing scraper");
                        PauseMopiScraper();
                    }
                    else if (!conflictingRunning && mScraperState == WebScraperState.Paused)
                    {
                        Log.Info("Conflicting program no longer running, resuming scraper");
                        ResumeMopiScraper();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in conflicting-check", ex);
            }
        }

        void Importer_MovieStatusChanged(MovieMatch obj, MovieImporterAction action)
        {
            try
            {
                if (obj != null)
                {
                    WebScraperItem item = null;
                    if (scraperItems.ContainsKey(obj))
                    {
                        item = scraperItems[obj];
                    }
                    else
                    {
                        item = new WebScraperItem();
                        item.ItemId = obj.Signature.MovieHash;
                        scraperItems.Add(obj, item);
                    }

                    List<WebScraperAction> actions = new List<WebScraperAction>();
                    actions.Add(new WebScraperAction() { ActionId = "send_importer", Title = "Send to Importer", Description = "Send this item to the importer", Enabled = (obj.Selected != null && action != MovieImporterAction.NEED_INPUT) });
                    item.ItemActions = actions;

                    item.State = action.ToString();

                    item.Title = obj.Signature.Title;

                    item.InputRequest = CreateInputRequest(obj);
                    item.LastUpdated = DateTime.Now;

                    Log.Info("Set {0} to {1}", item.Title, item.State);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in movingpictures Importer_MovieStatusChanged", ex);
            }
        }


        private WebScraperInputRequest CreateInputRequest(MovieMatch match)
        {
            WebScraperInputRequest inputRequest = new WebScraperInputRequest();
            inputRequest.Id = match.Signature.MovieHash;
            inputRequest.Title = match.LocalMediaString;
            if (match.PossibleMatches != null && match.PossibleMatches.Count > 0)
            {
                inputRequest.InputType = WebInputTypes.ItemSelect;
                inputRequest.InputOptions = new List<WebScraperInputMatch>();
                foreach (PossibleMatch m in match.PossibleMatches)
                {
                    inputRequest.InputOptions.Add(new WebScraperInputMatch()
                    {
                        Id = m.Movie.ImdbID,
                        Title = m.Movie.Title,
                        Description = m.Movie.Summary,
                        ImdbId = m.Movie.ImdbID,
                        FirstAired = new DateTime(m.Movie.Year, 1, 1)
                    });
                }
            }
            else
            {
                inputRequest.InputType = WebInputTypes.TextInput;
            }

            return inputRequest;
        }

        void Importer_Progress(int percentDone, int taskCount, int taskTotal, string taskDescription)
        {
            try
            {
                Log.Info(taskDescription + ", " + percentDone + "%");
                mCurrentAction = taskDescription;
                mCurrentPercentage = percentDone;
            }
            catch (Exception ex)
            {
                Log.Warn("Error in movingpictures Importer_Progress", ex);
            }
        }

        void MovingPicturesCore_InitializeProgress(string actionName, int percentDone)
        {
            try
            {
                Log.Debug(actionName + ", " + percentDone + "%");
                mCurrentAction = actionName;
                mCurrentPercentage = percentDone;
                if (percentDone == 100)
                {
                    MovingPicturesCore.Importer.Start();
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error in movingpictures MovingPicturesCore_InitializeProgress", ex);
            }
        }



        public void InitialiseScraper()
        {
            MovingPicturesCore.InitializeProgress += new ProgressDelegate(MovingPicturesCore_InitializeProgress);
            MovingPicturesCore.Importer.Progress += new MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieImporter.ImportProgressHandler(Importer_Progress);
            MovingPicturesCore.Importer.MovieStatusChanged += new MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieImporter.MovieStatusChangedHandler(Importer_MovieStatusChanged);
            MovingPicturesCore.DatabaseManager.ObjectUpdated += new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);


            // start initialization of the moving pictures core services in a seperate thread
            initThread = new Thread(new ThreadStart(MovingPicturesCore.Initialize));
            initThread.Start();
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
                scraperItems.Clear();

                matches = new List<MovieMatch>();
                mScraperState = WebScraperState.Running;
                InitialiseScraper();
                mImporterPausedManually = false;
                //Starting scraper importer
                MovingPicturesCore.Importer.Start();

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
                _conflictingChecker.Stop();
                Log.Debug(SCRAPER_LOG_PREFIX + "Stopping Scraper");
                mScraperState = WebScraperState.Stopped;
                //Stopping scraper importer
                MovingPicturesCore.Importer.Stop();
                MovingPicturesCore.Shutdown();

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
                return PauseMopiScraper();
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper not running");
                return false;
            }
        }

        private WebBoolResult PauseMopiScraper()
        {
            Log.Info("Pausing Scraper: " + SCRAPER_NAME);
            mScraperState = WebScraperState.Paused;
            MovingPicturesCore.Importer.Stop();
            return true;
        }

        public WebBoolResult ResumeScraper()
        {
            if (mScraperState == WebScraperState.Paused)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Resuming Scraper");
                mImporterPausedManually = false;
                return ResumeMopiScraper();
            }
            else
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper already running");
                return false;
            }
        }

        private WebBoolResult ResumeMopiScraper()
        {
            mScraperState = WebScraperState.Running;
            MovingPicturesCore.Importer.Start();
            return true;
        }

        void DatabaseManager_ObjectUpdated(Cornerstone.Database.Tables.DatabaseTable obj)
        {
            Log.Info("Updated: " + obj.ToString());
        }

        public WebBoolResult TriggerUpdate()
        {
            if (mScraperState == WebScraperState.Running)
            {
                Log.Debug(SCRAPER_LOG_PREFIX + "Scraper trigger update");
                MovingPicturesCore.Importer.RestartScanner();
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
                CurrentAction = mCurrentAction,
                CurrentProgress = mCurrentPercentage,
                InputNeeded = matches.Count,
                ScraperState = mScraperState
            };
        }

        public WebBoolResult SetScraperInputRequest(String requestId, String matchId, String text)
        {
            MovieMatch m = scraperItems.Where(x => x.Value.ItemId == requestId).First().Key;

            if (m != null)
            {
                if (text == null || text.Equals(""))
                {
                    if (m.PossibleMatches != null && m.PossibleMatches.Count > 0)
                    {
                        foreach (PossibleMatch p in m.PossibleMatches)
                        {
                            if (p.Movie.ImdbID.Equals(matchId))
                            {
                                PossibleMatch selectedMatch = p;
                                m.Selected = selectedMatch;
                                m.Signature.Title = p.DisplayMember;
                                break;
                            }
                        }
                        m.HighPriority = true;
                        MovingPicturesCore.Importer.Approve(m);
                        return true;
                    }
                }
                else
                {
                    m.Signature.Title = text;
                    MovingPicturesCore.Importer.Reprocess(m);
                    return true;
                }
            }

            return false;
        }


        public WebBoolResult AddItemToScraper(string title, WebMediaType type, int? provider, string itemId, int? offset)
        {
            return true;
        }


        public List<WebScraperItem> GetScraperItems()
        {
            return scraperItems.Values.ToList();
        }

        public List<WebScraperItem> GetUpdatedScraperItems(DateTime updated)
        {
            return scraperItems.Values.Where(x => x.LastUpdated > updated).ToList();
        }

        public WebScraperItem GetScraperItem(string itemId)
        {
            return scraperItems.Values.Where(x => x.ItemId == itemId).First();
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests()
        {
            List<WebScraperItem> updated = scraperItems.Values.Where(x => x.InputRequest != null).ToList();
            IList<WebScraperInputRequest> inputRequests = new List<WebScraperInputRequest>();

            foreach (WebScraperItem i in updated)
            {
                inputRequests.Add(i.InputRequest);
            }

            return inputRequests;
        }

        public List<WebScraperAction> GetScraperActions()
        {
            return new List<WebScraperAction>();
        }


        public WebBoolResult InvokeScraperAction(string itemId, string actionId)
        {
            MovieMatch m = scraperItems.Where(x => x.Value.ItemId == itemId).First().Key;
            if (actionId.Equals("send_importer"))
            {
                MovingPicturesCore.Importer.Reprocess(m);
            }
            return false;
        }

        public Form CreateConfig()
        {
            MovingPicturesConfig config = new MovingPicturesConfig();
            return config;
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
