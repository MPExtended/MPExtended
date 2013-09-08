#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MPExtended.Libraries.Client;
using System.Windows.Threading;
using MPExtended.Services.ScraperService.Interfaces;
using System.Collections.ObjectModel;
using System.ServiceModel;
using MPExtended.Libraries.Service;
using MPExtended.Applications.ServiceConfigurator.Code;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Composition;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabScraperConfig.xaml
    /// </summary>
    public partial class TabScraperConfig : Page, ITabCloseCallback
    {
        private DispatcherTimer _sessionWatcher;
        private ObservableCollection<WpfScraperConfig> _scrapers = new ObservableCollection<WpfScraperConfig>();
        private IScraperService proxyChannel;
        private IList<int> _autostart = new List<int>();

        private IScraperService Proxy
        {
            get
            {
                bool recreateChannel = false;
                if (proxyChannel == null)
                {
                    recreateChannel = true;
                }
                else if (((ICommunicationObject)proxyChannel).State == CommunicationState.Faulted)
                {
                    try
                    {
                        ((ICommunicationObject)proxyChannel).Close(TimeSpan.FromMilliseconds(500));
                    }
                    catch (Exception)
                    {
                        // oops. 
                    }

                    recreateChannel = true;
                }

                if (recreateChannel)
                {
                    var binding = new NetNamedPipeBinding()
                    {
                        MaxReceivedMessageSize = Int32.MaxValue
                    };

                    binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
                    binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

                    var endpointAddress = new EndpointAddress(String.Format("net.pipe://127.0.0.1/{0}", "MPExtended/ScraperService"));
                    var factory = new ChannelFactory<IScraperService>(binding, endpointAddress);

                    proxyChannel = factory.CreateChannel();

                }

                return proxyChannel;
            }
        }

        private class ProxyDomain : MarshalByRefObject
        {
            public Assembly GetAssembly(string assemblyPath)
            {
                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                catch (Exception ex)
                {
                    Log.Warn("Error on loading assembly " + assemblyPath, ex);
                }
                return null;
            }
        }

        public TabScraperConfig()
        {
            // service state watcher
            _sessionWatcher = new DispatcherTimer();
            _sessionWatcher.Interval = TimeSpan.FromSeconds(2);
            _sessionWatcher.Tick += new EventHandler(SessionWatcher_Tick);

            InitializeComponent();

            // actually show the list
            lvScrapers.ItemsSource = _scrapers;

            // start observing
            _sessionWatcher.Start();
        }

        public void TabClosed()
        {
            _sessionWatcher.Stop();
        }

        void SessionWatcher_Tick(object sender, EventArgs e)
        {
            try
            {
                IList<WebScraper> tmp = Proxy.GetAvailableScrapers();
                _autostart = Proxy.GetAutoStartPlugins();
                if (tmp != null)
                {
                    _scrapers.UpdateScraperList(tmp);
                }
            }
            catch (CommunicationException ex)
            {
                _scrapers.Clear();
                Log.Warn("No connection to service");
            }
            catch (Exception ex)
            {
                Log.Warn("Error updating scapers", ex);
            }
        }

        /// <summary>
        /// Prepare the context menu depending on the current state of the scraper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvScrapers_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
            if (scraper != null)
            {
                miStartScraper.IsEnabled = (scraper.ScraperInfo.ScraperState == WebScraperState.Stopped);
                miPauseResumeScraper.Header = (scraper.ScraperInfo.ScraperState == WebScraperState.Paused ? "Resume" : "Pause");
                miStopScraper.IsEnabled = (scraper.ScraperInfo.ScraperState != WebScraperState.Stopped);

                if(_autostart.Contains(scraper.ScraperId))
                {
                    miScraperAutostart.Header = "Disable Autostart";
                }
                else
                {
                    miScraperAutostart.Header = "Enable Autostart";
                }
            }
        }

        /// <summary>
        /// Start selected scraper (if stopped)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miStartScraper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
                if (scraper != null)
                {
                    bool success = Proxy.StartScraper(scraper.ScraperId);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error starting scraper", ex);
            }
        }

        /// <summary>
        /// Pause or resume the selected scraper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miPauseResumeScraper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
                if (scraper != null)
                {
                    if (scraper.ScraperInfo.ScraperState == WebScraperState.Paused)
                    {
                        Proxy.ResumeScraper(scraper.ScraperId);
                    }
                    else
                    {
                        Proxy.PauseScraper(scraper.ScraperId);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error pausing/resuming scraper", ex);
            }
        }

        /// <summary>
        /// Stop the selected scraper (if running or paused)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miStopScraper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
                if (scraper != null)
                {
                    bool success = Proxy.StopScraper(scraper.ScraperId);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error stopping scraper", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miScraperAutostart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
                if (scraper != null)
                {
                    bool auto = !_autostart.Contains(scraper.ScraperId);
                    if (Proxy.SetAutoStart(scraper.ScraperId, auto))
                    {
                        if (auto)
                        {
                            _autostart.Add(scraper.ScraperId);
                        }
                        else
                        {
                            _autostart.Remove(scraper.ScraperId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error stopping scraper", ex);
            }
        }

        /// <summary>
        /// Open a scraper config. We get the scraper information about the config form from the service, then use
        /// reflection to load the neccessary dll files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
                if (scraper != null)
                {
                    System.Windows.Forms.Application.EnableVisualStyles();
                    WebConfigResult scraperDll = Proxy.GetConfig(scraper.ScraperId);


                    if (File.Exists(scraperDll.DllPath))
                    {
                        //System.Reflection.Assembly myDllAssembly = System.Reflection.Assembly.LoadFile(formDll);
                        ProxyDomain pd = new ProxyDomain();
                        Assembly assembly = pd.GetAssembly(scraperDll.DllPath);
                        String assemblyName = scraperDll.PluginAssemblyName;

                        IScraperPlugin plugin = (IScraperPlugin)assembly.CreateInstance(assemblyName);
                        NativeAssemblyLoader.SetDllDirectory(new FileInfo(scraperDll.DllPath).Directory.FullName);

                        Form config = plugin.CreateConfig();


                        WebScraperState before = Proxy.GetScraperState(scraper.ScraperId).ScraperState;
                        if (before == WebScraperState.Running)
                        {
                            bool result = Proxy.PauseScraper(scraper.ScraperId);
                        }

                        config.ShowDialog();


                        if (before == WebScraperState.Running)
                        {
                            bool result = Proxy.ResumeScraper(scraper.ScraperId);
                        }

                    }
                    else
                    {
                        Log.Warn("Couldn't load scraper config, {0} doesn't exist", scraperDll.DllPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error starting scraper config", ex);
            }
        }

        /// <summary>
        /// Start all available scrapers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartAllScrapers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IList<WebScraper> scrapers = Proxy.GetAvailableScrapers();
                foreach (WebScraper s in scrapers)
                {
                    Proxy.StartScraper(s.ScraperId);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error starting scrapers", ex);
            }
        }

        /// <summary>
        /// Stop all available scrapers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopAllScrapers_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                IList<WebScraper> scrapers = Proxy.GetAvailableScrapers();
                foreach (WebScraper s in scrapers)
                {
                    Proxy.StopScraper(s.ScraperId);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error stopping scrapers", ex);
            }
        }
    }
}
