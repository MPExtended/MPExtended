using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using MPExtended.Services.ScraperService.Interfaces;
using MPExtended.PlugIns.Scrapers.MPTVSeries;
using System.Diagnostics;

namespace MPExtended.Scrapers.TVSeries
{
    class Program
    {
        private static MPTVSeriesScraper seriesImporter;
        static void Main(string[] args)
        {
            try
            {
                seriesImporter = new MPTVSeriesScraper();
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
                    else if (command.Equals("config"))
                    {
                       // seriesImporter.PauseScraper();

                        Process config = new Process();
                        config.StartInfo = new ProcessStartInfo("MPExtended.Scrapers.MPTVSeries.Config.exe");
                        config.Start();
                        config.WaitForExit();

                        //seriesImporter.ResumeScraper();
                    }

                    command = Console.ReadLine();
                }
                if (seriesImporter.IsRunning)
                {
                    seriesImporter.StopScraper();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start service");
            }
            Console.WriteLine("MPTVSeries Scraper closed");
        }
    }
}
