using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.ScraperService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IScraperPlugin
    {
        WebResult StartScraper();

        WebResult StopScraper();
        WebResult PauseScraper();
        WebResult ResumeScraper();
        WebResult TriggerUpdate();
        WebScraperStatus GetScraperStatus();
        WebScraperInputRequest GetScraperInputRequest(int index);
        IList<WebScraperInputRequest> GetAllScraperInputRequests();
        WebResult SetScraperInputRequest(String requestId, String matchId, String text);
        WebResult AddItemToScraper(String title, WebMediaType type, int? provider, string itemId, int? offset);
        List<WebScraperItem> GetScraperItems();
        List<WebScraperAction> GetScraperActions();
        WebBoolResult InvokeScraperAction(string itemId, string actionId);
        WebBoolResult ShowConfig();
    }
}