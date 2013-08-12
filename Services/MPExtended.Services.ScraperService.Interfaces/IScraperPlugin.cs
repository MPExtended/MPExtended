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
    /// Interface for a scraper plugin
    /// </summary>
    [ServiceContract(Namespace = "http://mpextended.github.com")]
    public interface IScraperPlugin
    {
        /// <summary>
        /// Start this scraper plugin
        /// </summary>
        /// <returns>Result of operation</returns>
        WebBoolResult StartScraper();

        /// <summary>
        /// Stop this scraper plugin
        /// </summary>
        /// <returns>Result of operation</returns>
        WebBoolResult StopScraper();

        /// <summary>
        /// Pause this scraper plugin
        /// </summary>
        /// <returns>Result of operation</returns>
        WebBoolResult PauseScraper();

        /// <summary>
        /// Resume this scraper plugin
        /// </summary>
        /// <returns>Result of operation</returns>
        WebBoolResult ResumeScraper();

        /// <summary>
        /// Trigger an update on this scraper, it's up to the implementation what
        /// is done (usually update the data)
        /// </summary>
        /// <returns>If scraper update worked</returns>
        WebBoolResult TriggerUpdate();

        /// <summary>
        /// Get the current status of this scraper plugin
        /// </summary>
        /// <returns>Status of scraper</returns>
        WebScraperInfo GetScraperState();

        /// <summary>
        /// Get the input request that are waiting for user input
        /// </summary>
        /// <returns>List of scraper input requests</returns>
        IList<WebScraperInputRequest> GetAllScraperInputRequests();

        /// <summary>
        /// Handle a input request when answered by the user
        /// </summary>
        /// <param name="requestId">Request Id</param>
        /// <param name="matchId">Match Id</param>
        /// <param name="text">Text (for free input)</param>
        /// <returns>If input request was handled successfully</returns>
        WebBoolResult SetScraperInputRequest(String requestId, String matchId, String text);
        
        /// <summary>
        /// Add an item to the scraper
        /// </summary>
        /// <param name="title">Title of item</param>
        /// <param name="type">Type of item</param>
        /// <param name="provider">Provider id</param>
        /// <param name="itemId">Id of item</param>
        /// <param name="offset">Item offset</param>
        /// <returns>Result of the operation (scraper plugin might not support this)</returns>
        WebBoolResult AddItemToScraper(String title, WebMediaType type, int? provider, string itemId, int? offset);
        
        /// <summary>
        /// Get all available scraper items
        /// </summary>
        /// <returns>List of scraper items</returns>
        List<WebScraperItem> GetScraperItems();

        /// <summary>
        /// Get all scraper items that have been updated since <paramref name="updated"/>
        /// </summary>
        /// <param name="updated">Updated timeframe for which items should be returned</param>
        /// <returns>List of scraper items</returns>
        List<WebScraperItem> GetUpdatedScraperItems(DateTime updated);

        /// <summary>
        /// Get a scraper item by id
        /// </summary>
        /// <param name="itemId">id of item</param>
        /// <returns>Scraper item</returns>
        WebScraperItem GetScraperItem(string itemId);

        /// <summary>
        /// Get the available custom actions for this scraper plugin
        /// </summary>
        /// <returns>List of available scraper actions</returns>
        List<WebScraperAction> GetScraperActions();

        /// <summary>
        /// Invoke a custom scraper action on this plugin
        /// </summary>
        /// <param name="itemId">Item id of action is associated with an item</param>
        /// <param name="actionId">Id of action</param>
        /// <returns>If the scraper aktion invoked successfully</returns>
        WebBoolResult InvokeScraperAction(string itemId, string actionId);

        /// <summary>
        /// If the scraper has a windows form config, create the config ui
        /// and return it
        /// </summary>
        /// <returns>Configuration UI for this plugin</returns>
        Form CreateConfig();

        /// <summary>
        /// Get the windows form config settings (path to external dlls, etc.)
        /// </summary>
        /// <returns>Settings for the plugins configuration ui</returns>
        WebConfigResult GetConfigSettings();
    }
}