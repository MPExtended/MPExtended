﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

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
        WebScraperStatus GetScraperStatus(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebScraperInputRequest GetScraperInputRequest(int? scraperId, int index);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        IList<WebScraperInputRequest> GetAllScraperInputRequests(int? scraperId);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        WebResult SetScraperInputRequest(int? scraperId, String requestId, String matchId, String text);
    }
}