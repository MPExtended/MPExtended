using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.ScraperService.Interfaces;
using MPExtended.Libraries.Service;
using System.Diagnostics;
using MPExtended.Libraries.Service.Hosting;
using System.IO;
using System.Reflection;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Libraries.Service.Composition;

namespace MPExtended.Services.ScraperService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class ScraperService : IScraperService
    {
        private Dictionary<int, Plugin<IScraperPlugin>> scrapers;
        public ScraperService()
        {
            scrapers = new Dictionary<int, Plugin<IScraperPlugin>>();

            var pluginLoader = new PluginLoader();
            pluginLoader.AddRequiredMetadata("Name");
            // first argument is directory name in source tree, second one in installed tree
            pluginLoader.AddFromTree("PlugIns", "Extensions");
            var plugins = pluginLoader.GetPlugins<IScraperPlugin>();

            foreach (var plugin in plugins)
            {
                Log.Debug("Loaded scraper plugin {0}", plugin.Metadata["Name"]);
                int id = (int)plugin.Metadata["Id"];
                scrapers.Add(id, plugin);
            }


            //StartMpTvSeriesCommandLine();

            //StartMopiCommandLine();

            //mDownloaderScraper = new ItemDownloader("lyfesaver.net:4322", "diebagger", "Addicted20");

            //ServiceState.Stopping += new ServiceState.ServiceStoppingEventHandler(ServiceState_Stopping);
        }
        /*
        /// <summary>
        /// Start the mp-tvseries command line scraper.
        /// </summary>
        private void StartMpTvSeriesCommandLine()
        {
            String file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\MpTvSeriesScraper\MPExtended.Scrapers.TVSeries.exe";
            //String file = @"C:\Users\DieBagger\Documents\Projects\MediaPortal\MpExt\git\Scrapers\MPExtended.Scrapers.MPTvSeries\bin\Debug\MPExtended.Scrapers.TVSeries.exe";
            if (File.Exists(file))
            {
                mTvSeriesCommandLine = new Process();
                mTvSeriesCommandLine.StartInfo.FileName = file;
                mTvSeriesCommandLine.StartInfo.UseShellExecute = false;
                mTvSeriesCommandLine.StartInfo.RedirectStandardInput = true;
                mTvSeriesCommandLine.StartInfo.RedirectStandardOutput = true;
                mTvSeriesCommandLine.StartInfo.RedirectStandardError = true;
                mTvSeriesCommandLine.EnableRaisingEvents = true;
                mTvSeriesCommandLine.Start();

                mTvSeriesCommandLine.OutputDataReceived += new DataReceivedEventHandler(mTvSeriesCommandLine_OutputDataReceived);
                mTvSeriesCommandLine.BeginOutputReadLine();
            }
            else
            {
                Log.Warn("Scraper not found: " + file);
            }
        }

        /// <summary>
        /// Start the moving-pictures command line scraper.
        /// </summary>
        private void StartMopiCommandLine()
        {
            String file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\MovingPicturesScraper\MPExtended.Scrapers.MovingPictures.exe";

            //String file = @"C:\Users\DieBagger\Documents\Projects\MediaPortal\MpExt\git\Scrapers\MPExtended.Scrapers.MovingPictures\bin\Debug\MPExtended.Scrapers.MovingPictures.exe";
            if (File.Exists(file))
            {
                String current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                mMopiCommandLine = new Process();
                mMopiCommandLine.StartInfo.FileName = file;
                mMopiCommandLine.StartInfo.UseShellExecute = false;
                mMopiCommandLine.StartInfo.RedirectStandardInput = true;
                mMopiCommandLine.StartInfo.RedirectStandardOutput = true;
                mMopiCommandLine.StartInfo.RedirectStandardError = true;
                mMopiCommandLine.EnableRaisingEvents = true;
                mMopiCommandLine.Start();

                mMopiCommandLine.OutputDataReceived += new DataReceivedEventHandler(mMopiCommandLine_OutputDataReceived);
                mMopiCommandLine.BeginOutputReadLine();
            }
            else
            {
                Log.Warn("Scraper not found: " + file);
            }
        }

        void mTvSeriesCommandLine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            String data = e.Data;
            Log.Debug("MP-TvSeries Scraper | " + data);
        }

        void mMopiCommandLine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            String data = e.Data;
            Log.Debug("MovingPictures Scraper | " + data);
        }

        private void StdOutReceived(object sender, DataReceivedEventArgs args)
        {

        }
        

        void ServiceState_Stopping()
        {
            //mTvSeriesCommandLine.StandardInput.WriteLine("exit");

            //mMopiCommandLine.StandardInput.WriteLine("exit");
        }

        ~ScraperService()
        {

        }
         * */

        private IScraperPlugin GetScraper(int? scraperId)
        {
            return scrapers[(int)scraperId].Value;
        }

        public IList<WebScraper> GetAvailableScrapers()
        {
            IList<WebScraper> returnList = new List<WebScraper>();
            foreach (Plugin<IScraperPlugin> s in scrapers.Values)
            {
                returnList.Add(new WebScraper()
                {
                    ScraperId = (int)s.Metadata["Id"],
                    ScraperName = (string)s.Metadata["Name"]
                });
            }

            return returnList;
        }

        public WebResult StartScraper(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.StartScraper();
        }

        public WebResult StopScraper(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.StopScraper();
        }

        public WebResult PauseScraper(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.PauseScraper();
        }

        public WebResult ResumeScraper(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.ResumeScraper();
        }

        public WebResult TriggerUpdate(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.TriggerUpdate();
        }

        public WebScraperStatus GetScraperStatus(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperStatus();
        }

        public WebScraperInputRequest GetScraperInputRequest(int? scraperId, int index)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperInputRequest(index);
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetAllScraperInputRequests();
        }

        public WebResult SetScraperInputRequest(int? scraperId, string requestId, string matchId, string text)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.SetScraperInputRequest(requestId, matchId, text);
        }


        public WebResult AddItemToScraper(int? scraperId, string title, WebMediaType type, int? provider, string itemId, int? offset)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.AddItemToScraper(title, type, provider, itemId, offset);
        }


        public List<WebScraperItem> GetScraperItems(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperItems();
        }

        public List<WebScraperAction> GetScraperActions(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperActions();
        }


        public WebBoolResult InvokeScraperAction(int? scraperId, string itemId, string actionId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.InvokeScraperAction(itemId, actionId);
        }


        public WebBoolResult ShowConfig(int? scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.ShowConfig();
        }
    }
}
