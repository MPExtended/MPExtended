using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.ScraperService.Interfaces;
using MPExtended.Scrapers.TVSeries;
using MPExtended.Libraries.Service;
using System.Diagnostics;
using MPExtended.Libraries.Service.Hosting;
using System.IO;

namespace MPExtended.Services.ScraperService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class ScraperService : IScraperService
    {
        private IPrivateScraperService CreateProxy(ref IPrivateScraperService service, String address)
        {
            bool recreateChannel = false;
            if (service == null)
            {
                recreateChannel = true;
            }
            else if (((ICommunicationObject)service).State == CommunicationState.Faulted)
            {
                try
                {
                    ((ICommunicationObject)service).Close(TimeSpan.FromMilliseconds(500));
                }
                catch (Exception)
                {
                    // oops. 
                }

                recreateChannel = true;
            }

            if (recreateChannel)
            {
                NetTcpBinding binding = new NetTcpBinding()
                {
                    MaxReceivedMessageSize = 100000000,
                    ReceiveTimeout = new TimeSpan(0, 0, 10),
                    SendTimeout = new TimeSpan(0, 0, 10),
                };
                binding.ReliableSession.Enabled = true;
                binding.ReliableSession.Ordered = true;

                service = ChannelFactory<IPrivateScraperService>.CreateChannel(
                    binding,
                    new EndpointAddress(address)
                );
            }

            return service;
        }

        private IPrivateScraperService mTvSeriesScraper;
        private IPrivateScraperService mMovingPicturesScraper;

        private Process mTvSeriesCommandLine;
        private Process mMopiCommandLine;

        public ScraperService()
        {
            mTvSeriesCommandLine = new Process();
            mTvSeriesCommandLine.StartInfo.FileName = @"C:\Users\DieBagger\Documents\Projects\MediaPortal\MpExt\git\Scrapers\MPExtended.Scrapers.MPTvSeries\bin\Debug\MPExtended.Scrapers.TVSeries.exe";
            //mTvSeriesCommandLine.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            mTvSeriesCommandLine.StartInfo.UseShellExecute = false;
            mTvSeriesCommandLine.StartInfo.RedirectStandardInput = true;
            mTvSeriesCommandLine.StartInfo.RedirectStandardOutput = true;
            mTvSeriesCommandLine.StartInfo.RedirectStandardError = true;
            mTvSeriesCommandLine.EnableRaisingEvents = true;
            mTvSeriesCommandLine.Start();

            mTvSeriesCommandLine.OutputDataReceived += new DataReceivedEventHandler(mTvSeriesCommandLine_OutputDataReceived);

            mTvSeriesCommandLine.BeginOutputReadLine();

            mMopiCommandLine = new Process();
            mMopiCommandLine.StartInfo.FileName = @"C:\Users\DieBagger\Documents\Projects\MediaPortal\MpExt\git\Scrapers\MPExtended.Scrapers.MovingPictures\bin\Debug\MPExtended.Scrapers.MovingPictures.exe";
            //mMopiCommandLine.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            mMopiCommandLine.StartInfo.UseShellExecute = false;
            mMopiCommandLine.StartInfo.RedirectStandardInput = true;
            mMopiCommandLine.StartInfo.RedirectStandardOutput = true;
            mMopiCommandLine.StartInfo.RedirectStandardError = true;
            mMopiCommandLine.EnableRaisingEvents = true;
            mMopiCommandLine.Start();

            mMopiCommandLine.OutputDataReceived += new DataReceivedEventHandler(mMopiCommandLine_OutputDataReceived);
            mMopiCommandLine.BeginOutputReadLine();

            ServiceState.Stopping += new ServiceState.ServiceStoppingEventHandler(ServiceState_Stopping);
        }

        void mTvSeriesCommandLine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            String data = e.Data;
            Log.Debug("MP-TvSeries Scraper | " + data);
        }

        void mMopiCommandLine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            String data = e.Data;
            Log.Debug("MovingPictures Scraper | " + data);
        }

        private void StdOutReceived(object sender, DataReceivedEventArgs args)
        {

        }

        void ServiceState_Stopping()
        {
            mTvSeriesCommandLine.StandardInput.WriteLine("exit");

            mMopiCommandLine.StandardInput.WriteLine("exit");
        }

        ~ScraperService()
        {

        }

        private IPrivateScraperService GetScraper(int? scraperId)
        {
            switch (scraperId)
            {
                case 0:
                    return CreateProxy(ref mTvSeriesScraper, "net.tcp://localhost:9760/MPExtended/ScraperServiceTVSeries");
                case 1:
                    return CreateProxy(ref mMovingPicturesScraper, "net.tcp://localhost:9761/MPExtended/ScraperServiceMopi");
                default:
                    return CreateProxy(ref mTvSeriesScraper, "net.tcp://localhost:9760/MPExtended/ScraperServiceTVSeries");
            }
        }

        public IList<WebScraper> GetAvailableScrapers()
        {
            IList<WebScraper> scrapers = new List<WebScraper>();
            AddScraper(scrapers, 0);
            AddScraper(scrapers, 1);

            return scrapers;
        }

        private void AddScraper(IList<WebScraper> scrapers, int scraperId)
        {
            try
            {
                WebScraper scraper = GetScraper(scraperId).GetScraperDescription();
                if (scraper != null)
                {
                    scrapers.Add(scraper);
                }
            }
            catch (Exception)
            {
                Log.Info("Scraper " + scraperId + " not running");
            }
        }


        public WebResult StartScraper(int? scraperId)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.StartScraper();
        }



        public WebResult StopScraper(int? scraperId)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.StopScraper();
        }

        public WebResult PauseScraper(int? scraperId)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.PauseScraper();
        }

        public WebResult ResumeScraper(int? scraperId)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.ResumeScraper();
        }

        public WebResult TriggerUpdate(int? scraperId)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.TriggerUpdate();
        }

        public WebScraperStatus GetScraperStatus(int? scraperId)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.GetScraperStatus();
        }

        public WebScraperInputRequest GetScraperInputRequest(int? scraperId, int index)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.GetScraperInputRequest(index);
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests(int? scraperId)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.GetAllScraperInputRequests();
        }

        public WebResult SetScraperInputRequest(int? scraperId, string requestId, string matchId, string text)
        {
            IPrivateScraperService service = GetScraper(scraperId);

            return service.SetScraperInputRequest(requestId, matchId, text);
        }
    }
}
