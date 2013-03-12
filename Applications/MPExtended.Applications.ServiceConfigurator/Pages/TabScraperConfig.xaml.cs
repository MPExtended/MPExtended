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

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabScraperConfig.xaml
    /// </summary>
    public partial class TabScraperConfig : Page, ITabCloseCallback
    {
        #region external calls
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AddDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetDllDirectory(int bufsize, StringBuilder buf);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string librayName);
        #endregion

        private DispatcherTimer _sessionWatcher;
        private ObservableCollection<WpfScraperConfig> _scrapers = new ObservableCollection<WpfScraperConfig>();
        private IScraperService proxyChannel;

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
            public Assembly GetAssembly(string AssemblyPath)
            {
                try
                {
                    return Assembly.LoadFrom(AssemblyPath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(ex.ToString());
                }
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

                if (tmp != null)
                {
                    //_scrapers.Upda
                    _scrapers.UpdateScraperList(tmp);
                }
            }
            catch (CommunicationException ex)
            {
                _scrapers.Clear();
                Log.Warn("No connection to service");
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
            }
        }

        /// <summary>
        /// Start selected scraper (if stopped)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miStartScraper_Click(object sender, RoutedEventArgs e)
        {
            WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
            if (scraper != null)
            {
                bool success = Proxy.StartScraper(scraper.ScraperId);
            }
        }

        /// <summary>
        /// Pause or resume the selected scraper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miPauseResumeScraper_Click(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Stop the selected scraper (if running or paused)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miStopScraper_Click(object sender, RoutedEventArgs e)
        {
            WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
            if (scraper != null)
            {
                bool success = Proxy.StopScraper(scraper.ScraperId);
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
            WebScraper scraper = (WebScraper)lvScrapers.SelectedItem;
            if (scraper != null)
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                WebConfigResult scraperDll = Proxy.GetConfig(scraper.ScraperId);


                if (File.Exists(scraperDll.DllPath))
                {
                    try
                    {
                        //System.Reflection.Assembly myDllAssembly = System.Reflection.Assembly.LoadFile(formDll);
                        ProxyDomain pd = new ProxyDomain();
                        Assembly assembly = pd.GetAssembly(scraperDll.DllPath);
                        String assemblyName = scraperDll.PluginAssemblyName;

                        IScraperPlugin plugin = (IScraperPlugin)assembly.CreateInstance(assemblyName);
                        SetDllDirectory(new FileInfo(scraperDll.DllPath).Directory.FullName);

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
                    catch (Exception ex)
                    {
                        Log.Error("Error starting scraper config", ex);
                    }
                }
                else
                {
                    Log.Warn("Couldn't load scraper config, {0} doesn't exist", scraperDll.DllPath);
                }
            }
        }

        /// <summary>
        /// Start all available scrapers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartAllScrapers_Click(object sender, RoutedEventArgs e)
        {
            IList<WebScraper> scrapers = Proxy.GetAvailableScrapers();
            foreach (WebScraper s in scrapers)
            {
                Proxy.StartScraper(s.ScraperId);
            }
        }

        /// <summary>
        /// Stop all available scrapers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopAllScrapers_Click(object sender, RoutedEventArgs e)
        {
            IList<WebScraper> scrapers = Proxy.GetAvailableScrapers();
            foreach (WebScraper s in scrapers)
            {
                Proxy.StopScraper(s.ScraperId);
            }
        }
    }
}
