using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.ScraperService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Scrapers.MediaManager
{
    public class MediaManager: IScraperPlugin
    {

        #region IPrivateScraperService overrides

        public WebResult StartScraper()
        {
            throw new NotImplementedException();
        }

        public WebResult StopScraper()
        {
            throw new NotImplementedException();
        }

        public WebResult PauseScraper()
        {
            throw new NotImplementedException();
        }

        public WebResult ResumeScraper()
        {
            throw new NotImplementedException();
        }

        public WebResult TriggerUpdate()
        {
            throw new NotImplementedException();
        }

        public WebScraperStatus GetScraperStatus()
        {
            throw new NotImplementedException();
        }

        public WebScraperInputRequest GetScraperInputRequest(int index)
        {
            throw new NotImplementedException();
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests()
        {
            throw new NotImplementedException();
        }

        public WebResult SetScraperInputRequest(string requestId, string matchId, string text)
        {
            throw new NotImplementedException();
        }
        
        public WebResult AddItemToScraper(string title, WebMediaType type, int? provider, string itemId, int? offset)
        {
            throw new NotImplementedException();
        }


        public List<WebScraperItem> GetScraperItems()
        {
            throw new NotImplementedException();
        }

        public List<WebScraperAction> GetScraperActions()
        {
            throw new NotImplementedException();
        }


        public WebBoolResult InvokeScraperAction(string itemId, string actionId)
        {
            throw new NotImplementedException();
        }
        
        public WebBoolResult ShowConfig()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
