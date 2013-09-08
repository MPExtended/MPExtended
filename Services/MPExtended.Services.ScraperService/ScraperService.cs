#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

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
            pluginLoader.AddRequiredMetadata("Id");
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

        #region IScraperService interface
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

        public WebBoolResult GetIsAutoStart(int scraperId)
        {
            return Configuration.Scraper.AutoStart.Contains(scraperId);
        }

        public IList<int> GetAutoStartPlugins()
        {
            return Configuration.Scraper.AutoStart;
        }

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
                    ScraperName = (string)s.Metadata["Name"],
                    ScraperInfo = s.Value.GetScraperState()
                });
            }

            return returnList;
        }

        public WebBoolResult StartScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.StartScraper();
        }

        public WebBoolResult StopScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.StopScraper();
        }

        public WebBoolResult PauseScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.PauseScraper();
        }

        public WebBoolResult ResumeScraper(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.ResumeScraper();
        }

        public WebBoolResult TriggerUpdate(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.TriggerUpdate();
        }

        public WebScraperInfo GetScraperState(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperState();
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

        public IList<WebScraperItem> GetScraperItems(int scraperId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperItems();
        }

        public WebScraperItem GetScraperItem(int scraperId, string itemId)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetScraperItem(itemId);
        }

        public IList<WebScraperItem> GetUpdatedScraperItems(int scraperId, DateTime updated)
        {
            IScraperPlugin service = GetScraper(scraperId);

            return service.GetUpdatedScraperItems(updated);
        }

        public IList<WebScraperAction> GetScraperActions(int scraperId)
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

            return service.GetConfigSettings();
        }
        #endregion
    }
}
