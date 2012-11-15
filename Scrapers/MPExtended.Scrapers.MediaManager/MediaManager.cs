using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Scrapers.MediaManager
{
    public class MediaManager: IPrivateScraperService
    {
        #region IPrivateScraperService overrides
        public WebScraper GetScraperDescription()
        {
            throw new NotImplementedException();
        }

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
        #endregion


    }
}
