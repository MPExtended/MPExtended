﻿using System;
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
        WebResult StartScraper(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResult StopScraper(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResult PauseScraper(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResult ResumeScraper(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResult TriggerUpdate(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScraperInfo GetScraperState(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResult SetScraperInputRequest(int? scraperId, String requestId, String matchId, String text);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResult AddItemToScraper(int? scraperId, string title, WebMediaType type, int? provider, string itemId, int? offset);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebScraperItem> GetScraperItems(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebScraperItem> GetUpdatedScraperItems(int? scraperId, DateTime updated);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScraperItem GetScraperItem(int? scraperId, string itemId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        List<WebScraperAction> GetScraperActions(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebBoolResult InvokeScraperAction(int? scraperId, string itemId, string actionId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebConfigResult GetConfig(int? scraperId);
    }
}