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
    public interface IScraperService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScraper> GetAvailableScrapers();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult StartScraper(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult StopScraper(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult PauseScraper(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult ResumeScraper(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult TriggerUpdate(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScraperInfo GetScraperState(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult SetScraperInputRequest(int scraperId, String requestId, String matchId, String text);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult AddItemToScraper(int scraperId, string title, WebMediaType type, int? provider, string itemId, int? offset);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebScraperItem> GetScraperItems(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebScraperItem> GetUpdatedScraperItems(int scraperId, DateTime updated);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScraperItem GetScraperItem(int scraperId, string itemId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebScraperAction> GetScraperActions(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult InvokeScraperAction(int scraperId, string itemId, string actionId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebConfigResult GetConfig(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult SetAutoStart(int scraperId, bool autoStart);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult GetIsAutoStart(int scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<int> GetAutoStartPlugins();
    }
}