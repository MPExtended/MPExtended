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
    /// <summary>
    /// Service that runs scraper plugins that do small tasks on the
    /// server (like updating meta data)
    /// </summary>
    [ServiceContract(Namespace = "http://mpextended.github.io")]
    public interface IScraperService
    {
        /// <summary>
        /// Get a list of all available scrapers installed on the server
        /// </summary>
        /// <returns>List of scraper plugins</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScraper> GetAvailableScrapers();

        /// <summary>
        /// Start scraper plugin
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>If the scraper started successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult StartScraper(int scraperId);

        /// <summary>
        /// Stop scraper plugin
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>If the scraper stopped successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult StopScraper(int scraperId);

        /// <summary>
        /// Pause scraper plugin
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>If the scraper paused successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult PauseScraper(int scraperId);

        /// <summary>
        /// Resume scraper plugin
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>If the scraper resumed successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult ResumeScraper(int scraperId);

        /// <summary>
        /// Trigger scraper plugin update
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>If the scraper updated successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult TriggerUpdate(int scraperId);

        /// <summary>
        /// Get the current state of this scraper plugin
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>Status of scraper</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScraperInfo GetScraperState(int scraperId);

        /// <summary>
        /// Get the input request that are waiting for user input
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>List of scraper input requests</returns>
        IList<WebScraperInputRequest> GetAllScraperInputRequests(int scraperId);

        /// <summary>
        /// Handle a input request when answered by the user
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <param name="requestId">Request Id</param>
        /// <param name="matchId">Match Id</param>
        /// <param name="text">Text (for free input)</param>
        /// <returns>If input request was handled successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult SetScraperInputRequest(int scraperId, String requestId, String matchId, String text);

        /// <summary>
        /// Add an item to the scraper
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <param name="title">Title of item</param>
        /// <param name="type">Type of item</param>
        /// <param name="provider">Provider id</param>
        /// <param name="itemId">Id of item</param>
        /// <param name="offset">Item offset</param>
        /// <returns>Result of the operation (scraper plugin might not support this)</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult AddItemToScraper(int scraperId, string title, WebMediaType type, int? provider, string itemId, int? offset);

        /// <summary>
        /// Get all available scraper items
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>List of scraper items</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScraperItem> GetScraperItems(int scraperId);

        /// <summary>
        /// Get all scraper items that have been updated since <paramref name="updated"/>
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <param name="updated">Updated timeframe for which items should be returned</param>
        /// <returns>List of scraper items</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScraperItem> GetUpdatedScraperItems(int scraperId, DateTime updated);

        /// <summary>
        /// Get a scraper item by id
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <param name="itemId">id of item</param>
        /// <returns>Scraper item</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScraperItem GetScraperItem(int scraperId, string itemId);

        /// <summary>
        /// Get the available custom actions for this scraper plugin
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>List of available scraper actions</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScraperAction> GetScraperActions(int scraperId);


        /// <summary>
        /// Invoke a custom scraper action on the given plugin
        /// </summary>
        /// <param name="itemId">Item id of action is associated with an item</param>
        /// <param name="actionId">Id of action</param>
        /// <returns>If the scraper aktion invoked successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult InvokeScraperAction(int scraperId, string itemId, string actionId);

        /// <summary>
        /// If the scraper has a windows form config, get the config ui
        /// settings. To actually show the ui, an application has to use
        /// reflection to load the neccessary assemblies (as provided by
        /// the returned <paramref name="scraperId"/> object) and then
        /// use the CreateConfig method of the IScraperPlugin object.
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>Configuration UI for this plugin</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebConfigResult GetConfig(int scraperId);

        /// <summary>
        /// Set scraper plugin autostart status
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>If the scraper started successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult SetAutoStart(int scraperId, bool autoStart);

        /// <summary>
        /// Get scraper plugin autostart status
        /// </summary>
        /// <param name="scraperId">Id of scraper</param>
        /// <returns>If the scraper started successfully</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult GetIsAutoStart(int scraperId);

        /// <summary>
        /// Get scraper plugins that have autostart enabled
        /// </summary>
        /// <returns>List of scrapers that are automatically started</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<int> GetAutoStartPlugins();
    }
}