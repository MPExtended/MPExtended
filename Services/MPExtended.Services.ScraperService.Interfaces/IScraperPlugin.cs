using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using MPExtended.Services.Common.Interfaces;
using System.Windows.Forms;

namespace MPExtended.Services.ScraperService.Interfaces
{
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IScraperPlugin
    {
        WebBoolResult StartScraper();
        WebBoolResult StopScraper();
        WebBoolResult PauseScraper();
        WebBoolResult ResumeScraper();
        WebBoolResult TriggerUpdate();
        WebScraperInfo GetScraperStatus();
        IList<WebScraperInputRequest> GetAllScraperInputRequests();
        WebBoolResult SetScraperInputRequest(String requestId, String matchId, String text);
        WebBoolResult AddItemToScraper(String title, WebMediaType type, int? provider, string itemId, int? offset);
        List<WebScraperItem> GetScraperItems();
        List<WebScraperItem> GetUpdatedScraperItems(DateTime updated);
        WebScraperItem GetScraperItem(string itemId);
        List<WebScraperAction> GetScraperActions();
        WebBoolResult InvokeScraperAction(string itemId, string actionId);
        Form CreateConfig();
        WebConfigResult GetConfig();
    }
}