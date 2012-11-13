using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MediaPortal.Plugins.MovingPictures.LocalMediaManagement;
using MediaPortal.Plugins.MovingPictures;
using Cornerstone.GUI.Dialogs;
using System.Diagnostics;
using System.ServiceModel;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Scrapers.MovingPictures
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class MovingPicturesImporter : IPrivateScraperService
    {
        private static int SCRAPER_ID = 1;
        private static String SCRAPER_NAME = "MovingPictures Importer";

        private void CheckConflictingProgramsRunning()
        {
            while (mScraperState != WebScraperState.Stopped)
            {
                if (!mImporterPausedManually)
                {
                    bool conflictingRunning = false;
                    Process config = GetProcess("Configuration");

                    if (config != null)
                    {
                        if (config.MainWindowTitle != null)
                        {
                            conflictingRunning = true;
                        }
                    }

                    Process mp = GetProcess("MediaPortal");
                    if (mp != null)
                    {
                        conflictingRunning = true;
                    }

                    Process mopiConfig = GetProcess("MPExtended.Scrapers.MovingPictures.Config");
                    if (mopiConfig != null)
                    {
                        conflictingRunning = true;
                    }

                    if (conflictingRunning)
                    {
                        if (mScraperState != WebScraperState.Paused)
                        {
                            Console.WriteLine("Conflicting program running, pausing scraper");

                            PauseMopiScraper();
                        }
                    }
                    else
                    {
                        if (mScraperState == WebScraperState.Paused)
                        {
                            Console.WriteLine("Conflicting program no longer running, resuming scraper");
                            ResumeMopiScraper();
                        }

                    }
                }

                Thread.Sleep(1000);
            }
        }

        private Thread initThread;
        private List<MovieMatch> matches = new List<MovieMatch>();
        private Thread conflictingCheckThread;
        private string mCurrentAction;
        private int mCurrentPercentage;
        private bool mImporterPausedManually;
        private WebScraperState mScraperState = WebScraperState.Stopped;

        public MovingPicturesImporter()
        {
            MovingPicturesCore.InitializeProgress += new ProgressDelegate(MovingPicturesCore_InitializeProgress);
            MovingPicturesCore.Importer.Progress += new MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieImporter.ImportProgressHandler(Importer_Progress);
            MovingPicturesCore.Importer.MovieStatusChanged += new MediaPortal.Plugins.MovingPictures.LocalMediaManagement.MovieImporter.MovieStatusChangedHandler(Importer_MovieStatusChanged);
            MovingPicturesCore.DatabaseManager.ObjectUpdated += new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(DatabaseManager_ObjectUpdated);
        }

        void Importer_MovieStatusChanged(MovieMatch obj, MovieImporterAction action)
        {
            if (action == MovieImporterAction.NEED_INPUT)
            {
                matches.Add(obj);
            }
            else if (action == MovieImporterAction.COMMITED)
            {
                foreach (MovieMatch m in matches)
                {
                    if (m.Signature.MovieHash.Equals(obj.Signature.MovieHash))
                    {
                        matches.Remove(m);
                        break;
                    }
                }
            }
            Console.WriteLine(action.ToString());
            //Console.WriteLine("[update]");
            mCurrentAction = (obj != null ? (obj.ToString() + " ") : "") + action.ToString();
            mCurrentPercentage = 100;
        }

        void Importer_Progress(int percentDone, int taskCount, int taskTotal, string taskDescription)
        {
            Console.WriteLine(taskDescription + ", " + percentDone + "%");
            mCurrentAction = taskDescription;
            mCurrentPercentage = percentDone;
        }

        void MovingPicturesCore_InitializeProgress(string actionName, int percentDone)
        {
            Console.WriteLine(actionName + ", " + percentDone + "%");
            mCurrentAction = actionName;
            mCurrentPercentage = percentDone;
            if (percentDone == 100)
            {
                MovingPicturesCore.Importer.Start();
            }
        }

        public Process GetProcess(string name)
        {
            try
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //otherwise we return a false
            return null;
        }

        public void InitialiseScraper()
        {
            // start initialization of the moving pictures core services in a seperate thread
            initThread = new Thread(new ThreadStart(MovingPicturesCore.Initialize));
            initThread.Start();

            conflictingCheckThread = new Thread(new ThreadStart(CheckConflictingProgramsRunning));
            conflictingCheckThread.Start();
        }

        public WebScraper GetScraperDescription()
        {
            return new WebScraper()
            {
                ScraperId = SCRAPER_ID,
                ScraperName = SCRAPER_NAME
            };
        }

        public WebResult StartScraper()
        {
            Console.WriteLine("Starting MovingPictures Scraper");
            matches = new List<MovieMatch>();
            mScraperState = WebScraperState.Running;
            InitialiseScraper();
            mImporterPausedManually = false;
            //Starting scraper importer
            MovingPicturesCore.Importer.Start();
            return true;
        }

        public WebResult StopScraper()
        {
            Console.WriteLine("Stopping MovingPictures Scraper");
            mScraperState = WebScraperState.Stopped;
            //Stopping scraper importer
            MovingPicturesCore.Importer.Stop();
            MovingPicturesCore.Shutdown();
            return true;
        }

        public WebResult PauseScraper()
        {
            mImporterPausedManually = true;
            return PauseMopiScraper();
        }

        private WebResult PauseMopiScraper()
        {
            Console.WriteLine("Pausing MovingPictures Scraper");
            mScraperState = WebScraperState.Paused;
            MovingPicturesCore.Importer.Stop();
            return true;
        }

        public WebResult ResumeScraper()
        {
            mImporterPausedManually = false;
            return ResumeMopiScraper();
        }

        void DatabaseManager_ObjectUpdated(Cornerstone.Database.Tables.DatabaseTable obj)
        {
            Console.WriteLine("Updated: " + obj.ToString());
        }

        private WebResult ResumeMopiScraper()
        {
            Console.WriteLine("Resuming MovingPictures Scraper");
            mScraperState = WebScraperState.Running;
            MovingPicturesCore.Importer.Start();
            return true;
        }

        public WebResult TriggerUpdate()
        {
            MovingPicturesCore.Importer.RestartScanner();
            return true;
        }

        public WebScraperStatus GetScraperStatus()
        {
            return new WebScraperStatus()
            {
                CurrentAction = mCurrentAction,
                CurrentProgress = mCurrentPercentage,
                InputNeeded = matches.Count,
                ScraperState = mScraperState
            };
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests()
        {
            List<WebScraperInputRequest> returnList = new List<WebScraperInputRequest>();
            for (int i = 0; i < matches.Count; i++)
            {
                returnList.Add(GetScraperInputRequest(i));
            }
            return returnList;
        }

        public WebScraperInputRequest GetScraperInputRequest(int index)
        {
            MovieMatch match = matches[index];
            WebScraperInputRequest inputRequest = new WebScraperInputRequest();
            if (match.PossibleMatches != null && match.PossibleMatches.Count > 0)
            {
                inputRequest.Title = match.LocalMediaString;
                inputRequest.InputType = WebInputTypes.ItemSelect;
                inputRequest.InputOptions = new List<WebScraperInputMatch>();
                inputRequest.Id = match.Signature.MovieHash;
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

        public WebResult SetScraperInputRequest(String requestId, String matchId, String text)
        {
            foreach (MovieMatch m in matches)
            {
                if (m.Signature.MovieHash.Equals(requestId))
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
            }
            return false;
        }
    }
}
