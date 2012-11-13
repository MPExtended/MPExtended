using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceModel;
using MPExtended.Libraries.Service;
using System.Diagnostics;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Scrapers.MovingPictures
{
    static class Program
    {
        private static ServiceHost scraperHost;
        private static MovingPicturesImporter importer;
        static void Main(string[] args)
        {
            try
            {
                importer = new MovingPicturesImporter();
                scraperHost = new ServiceHost(importer);

                scraperHost.Open();
                Console.WriteLine("Opened ServiceHost...");
                PrintPossibleCommands();

                String command = Console.ReadLine();
                while (!command.Equals("exit"))
                {
                    if (command.Equals("pause"))
                    {
                        importer.PauseScraper();
                    }
                    else if (command.Equals("resume"))
                    {
                        importer.ResumeScraper();
                    }
                    else if (command.Equals("start"))
                    {
                        importer.StartScraper();
                    }
                    else if (command.Equals("stop"))
                    {
                        importer.StopScraper();
                    }
                    else if (command.Equals("status"))
                    {
                        WebScraperStatus status = importer.GetScraperStatus();

                        if (status != null)
                        {
                            Console.WriteLine(status.CurrentAction + " - " + status.CurrentProgress + " (Input needed: " + status.InputNeeded + ")");
                        }
                    }
                    else if (command.Equals("inputs"))
                    {
                        WebScraperStatus status = importer.GetScraperStatus();

                        if (status != null)
                        {
                            for (int i = 0; i < status.InputNeeded; i++)
                            {

                                WebScraperInputRequest request = importer.GetScraperInputRequest(i);
                                if (request != null)
                                {
                                    Console.WriteLine(i + ": " + request.Title + " (" + request.InputType.ToString() + ")");
                                }
                            }
                        }
                    }
                    else if (command.Equals("match"))
                    {
                        Console.WriteLine("index of match: ");
                        int index = Int32.Parse(Console.ReadLine());

                        WebScraperInputRequest request = importer.GetScraperInputRequest(index);
                        if (request != null)
                        {
                            Console.WriteLine(request.Title);
                            Console.WriteLine(request.Text);
                            if (request.InputType == WebInputTypes.ItemSelect)
                            {
                                for (int i = 0; i < request.InputOptions.Count; i++)
                                {
                                    Console.WriteLine(i + ": " + request.InputOptions[i]);
                                }
                            }

                            int selectedindex = Int32.Parse(Console.ReadLine());
                            if (selectedindex >= 0 && selectedindex < request.InputOptions.Count)
                            {
                                importer.SetScraperInputRequest(request.Id, request.InputOptions[selectedindex].Id, request.InputOptions[selectedindex].Title);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No match at that index");
                        }
                    }
                    else if (command.Equals("config"))
                    {
                        importer.PauseScraper();
                        //Process.Start();
                        Process config = new Process();
                        config.StartInfo = new ProcessStartInfo("MPExtended.Scrapers.MovingPictures.Config.exe");
                        config.Start();
                        config.WaitForExit();
                        importer.ResumeScraper();
                    }
                    else if (command.Equals("help"))
                    {
                        PrintPossibleCommands();
                    }

                    command = Console.ReadLine();
                }
                if (importer.GetScraperStatus().ScraperState != WebScraperState.Stopped)
                {
                    importer.StopScraper();
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
            Console.WriteLine("Moving Pictures Scraper closed");
        }

        private static void PrintPossibleCommands()
        {
            Console.WriteLine("Available Commands:");
            Console.WriteLine("start:   starts scraper");
            Console.WriteLine("stop:    stop scraper");
            Console.WriteLine("pause:   pause scraper");
            Console.WriteLine("resume:  resumes scraper");
            Console.WriteLine("status:  show scraper status");
            Console.WriteLine("inputs:  show matches that need user input");
            Console.WriteLine("match:   confirm a close-resolution match");
            Console.WriteLine("help:    shows available commands");
            Console.WriteLine("config:  opens config");
            Console.WriteLine("exit:    exits scraper");
        }
    }
}
