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
using System.Windows.Forms;

namespace MPExtended.Services.ScraperService
{
    /// <summary>
    /// ScraperService is a service for running scrapers. A scraper is a process that updates
    /// the data while the service is running. The scraper service allows start/stop scrapers and
    /// a limited interaction. 
    /// </summary>
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
            //pluginLoader.AddFromTree("PlugIns", "Extensions");
            pluginLoader.AddFromTreeMatch(@"PlugIns\MPExtended.PlugIns.Scrapers.*", @"Plugins\Scrapers");
            var plugins = pluginLoader.GetPlugins<IScraperPlugin>();

            foreach (var plugin in plugins)
            {
                Log.Debug("Loaded scraper plugin {0}", plugin.Metadata["Name"]);
                int id = (int)plugin.Metadata["Id"];
                scrapers.Add(id, plugin);

                if (Configuration.Scraper.AutoStart != null &&
                    Configuration.Scraper.AutoStart.Contains(id))
                {
                    Log.Debug("Auto-Starting plugin " + plugin.Metadata["Name"]);
                    plugin.Value.StartScraper();
                }
            }
        }

        public WebBoolResult SetAutoStart(int scraperId, bool autoStart)
        {
            if (autoStart && !Configuration.Scraper.AutoStart.Contains(scraperId))
            {
                Configuration.Scraper.AutoStart.Add(scraperId);
                return Configuration.Save();
            }
            else if (!autoStart && Configuration.Scraper.AutoStart.Contains(scraperId))
            {
                Configuration.Scraper.AutoStart.Remove(scraperId);
                return Configuration.Save(); ;
            }
            return false;
        }

        /// <summary>
        /// Get 
        /// </summary>
        /// <param name="scraperId"></param>
        /// <returns></returns>
        public WebBoolResult GetIsAutoStart(int scraperId)
        {
            return Configuration.Scraper.AutoStart.Contains(scraperId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<int> GetAutoStartPlugins()
        {
            return Configuration.Scraper.AutoStart;
        }

        /// <summary>
        /// Return scraper by id
        /// </summary>
        /// <param name="scraperId">id of scraper</param>
        /// <returns>Scraper objects</returns>
        private IScraperPlugin GetScraper(int? scraperId)
        {
            return scrapers[(int)scraperId].Value;
        }

        /// <summary>
        /// Get all available scraper of this system
        /// </summary>
        /// <returns>Available scrapers</returns>
        public IList<WebScraper> GetAvailableScrapers()
        {
            IList<WebScraper> returnList = new List<WebScraper>();
            foreach (Plugin<IScraperPlugin> s in scrapers.Values)
            {
                returnList.Add(new WebScraper()
                {
                    ScraperId = (int)s.Metadata["Id"],
                    ScraperName = (string)s.Metadata["Name"],
                    ScraperInfo = s.Value.GetScraperStatus()
                });
            }

            return returnList;
        }

        /// <summary>
        /// Start a scraper
        /// </summary>
        /// <param name="scraperId">id of scraper</param>
        /// <returns>true if scraper could be started, false otherwise</returns>
        public WebBoolResult StartScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.StartScraper();
        }

        /// <summary>
        /// Stop a scraper
        /// </summary>
        /// <param name="scraperId">id of scraper</param>
        /// <returns>true if scraper could be stopped, false otherwise</returns>
        public WebBoolResult StopScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.StopScraper();
        }

        /// <summary>
        /// Pause a scraper
        /// </summary>
        /// <param name="scraperId">id of scraper</param>
        /// <returns>true if scraper could be paused, false otherwise</returns>
        public WebBoolResult PauseScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.PauseScraper();
        }

        /// <summary>
        /// Resume a scraper
        /// </summary>
        /// <param name="scraperId">id of scraper</param>
        /// <returns>true if scraper could be resumed, false otherwise</returns>
        public WebBoolResult ResumeScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.ResumeScraper();
        }

        /// <summary>
        /// Trigger an update on a scraper
        /// </summary>
        /// <param name="scraperId">id of scraper</param>
        /// <returns>true if scraper update could be triggered, false otherwise</returns>
        public WebBoolResult TriggerUpdate(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.TriggerUpdate();
        }

        /// <summary>
        /// Get current state of a scraper
        /// </summary>
        /// <param name="scraperId">id of scraper</param>
        /// <returns>State of scraper</returns>
        public WebScraperInfo GetScraperState(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperStatus();
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetAllScraperInputRequests();
        }

        public WebBoolResult SetScraperInputRequest(int scraperId, string requestId, string matchId, string text)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.SetScraperInputRequest(requestId, matchId, text);
        }

        public WebBoolResult AddItemToScraper(int scraperId, string title, WebMediaType type, int? provider, string itemId, int? offset)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.AddItemToScraper(title, type, provider, itemId, offset);
        }

        public List<WebScraperItem> GetScraperItems(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperItems();
        }

        public WebScraperItem GetScraperItem(int scraperId, string itemId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperItem(itemId);
        }

        public List<WebScraperItem> GetUpdatedScraperItems(int scraperId, DateTime updated)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetUpdatedScraperItems(updated);
        }

        public List<WebScraperAction> GetScraperActions(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperActions();
        }

        public WebBoolResult InvokeScraperAction(int scraperId, string itemId, string actionId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.InvokeScraperAction(itemId, actionId);
        }

        public WebConfigResult GetConfig(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetConfig();
        }
    }
}
