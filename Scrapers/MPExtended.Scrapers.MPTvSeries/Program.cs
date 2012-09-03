using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Scrapers.TVSeries
{
    class Program
    {
        private static ServiceHost scraperHost;
        private static TVSeriesImporter seriesImporter;
        static void Main(string[] args)
        {
            try
            {
                seriesImporter = new TVSeriesImporter();
                scraperHost = new ServiceHost(seriesImporter);
                
                scraperHost.Open();
                Console.WriteLine("Opened ServiceHost...");

                String command = Console.ReadLine();

                while (!command.Equals("exit"))
                {
                    if (command.Equals("pause"))
                    {
                        seriesImporter.PauseScraper();
                    }
                    else if (command.Equals("unpause"))
                    {
                        seriesImporter.ResumeScraper();
                    }
                    else if (command.Equals("start"))
                    {
                        seriesImporter.StartScraper();
                    }
                    else if (command.Equals("stop"))
                    {
                        seriesImporter.StopScraper();
                    }
                    else if (command.Equals("status"))
                    {
                        WebScraperStatus status = seriesImporter.GetScraperStatus();

                        if (status != null)
                        {
                            Console.WriteLine(status.CurrentAction + " - " + status.CurrentProgress + " (Input needed: " + status.InputNeeded + ")");
                        }
                    }
                    else if (command.Equals("inputs"))
                    {
                        WebScraperStatus status = seriesImporter.GetScraperStatus();

                        if (status != null)
                        {
                            for (int i = 0; i < status.InputNeeded; i++)
                            {
                               
                                WebScraperInputRequest request = seriesImporter.GetScraperInputRequest(i);
                                if (request != null)
                                {
                                    Console.WriteLine(i + ": " + request.Title + "(" + request.InputType.ToString() + ")");
                                }
                            }
                        }
                    }

                    command = Console.ReadLine();
                }
                if (seriesImporter.IsRunning)
                {
                    seriesImporter.StopScraper();
                }
                scraperHost.Close();
            }
            catch (AddressAlreadyInUseException)
            {
                Console.WriteLine("Address for ScraperService is already in use");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start service");
            }
            Console.WriteLine("Mopi Scraper closed");
        }
    }
}
