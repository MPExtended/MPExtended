#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.ComponentModel;
using System.IO;
using System.ServiceProcess;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Documents;
using System.Windows.Threading;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Meta;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using System.Xml.Linq;
using MPExtended.Services.MediaAccessService;
using System.Diagnostics;
using System.Security.Principal;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using Microsoft.Win32;
using MPExtended.Applications.ServiceConfigurator.Code;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;


namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for MediaAccessServer.xaml
    /// </summary>
    public partial class MediaAccessServer : Page
    {
        private ServiceController mServiceController;
        private DispatcherTimer mServiceWatcher;
        private DispatcherTimer mSessionWatcher;
        private DispatcherTimer mLogUpdater;

        private ObservableCollection<WpfStreamingSession> mStreamingSessions = new ObservableCollection<WpfStreamingSession>();
        private Timer activeSessionTimer = new Timer();
        private InstallationType mInstallationType = Installation.GetInstallationType();

        Dictionary<String, Dictionary<String, PluginConfigItem>> PluginConfigurations { get; set; }

        private XElement mConfigFile;

        private string mSelectedLog;

        public MediaAccessServer()
        {
            InitializeComponent();

            SetConfigLocation();
            GenerateBarcode(checkBox1.IsChecked == true);

            InitLogFiles();

            txtServiceName.Text = GetServiceName();

            InitServiceConfiguration();

            try
            {
                mServiceController = new ServiceController("MPExtended Service");
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("MPExtended service is not installed! Please install the latest version.");
                return;
            }

            //Load the MediaAccess.xml configuration file

            //WebBackendConfiguration backendconfig = MPEServices.NetPipeMediaAccessService.GetBackendConfiguration();

            PluginConfigurations = new Dictionary<String, Dictionary<String, PluginConfigItem>>();
            try
            {
                mConfigFile = XElement.Load(Configuration.GetPath("MediaAccess.xml"));
                var config = mConfigFile.Element("pluginConfiguration").Elements("plugin");

                foreach (var p in config)
                {
                    Dictionary<String, PluginConfigItem> props = new Dictionary<String, PluginConfigItem>();
                    foreach (var p2 in p.Descendants())
                    {
                        props.Add(p2.Name.LocalName, new PluginConfigItem(p2) { Tag = p2 });
                        //.ToDictionary(x => x.Key, x => PerformFolderSubstitution(x.Value));
                    }
                    PluginConfigurations.Add(p.Attribute("name").Value, props);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading plugin config");
                Log.Error(ex.ToString());
            }

            cbPluginConfigs.DataContext = PluginConfigurations;
            cbPluginConfigs.DisplayMemberPath = "Key";
            cbPluginConfigs.SelectedValuePath = "Value";

            if (!isServiceAvailable(mServiceController))
            {
                mServiceController = null;
            }

            lvActiveStreams.ItemsSource = mStreamingSessions;

            InitBackgroundWorker();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void InitLogFiles()
        {
            String logRoot = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MPExtended\\Logs";

            if (File.Exists(logRoot + "\\Service.log"))
            {
                cbLogFiles.Items.Add("Service");
            }

            if (cbLogFiles.Items.Count > 0)
            {
                cbLogFiles.SelectedIndex = 0;
                mSelectedLog = (String)cbLogFiles.SelectedItem;
            }
        }

        private bool isServiceAvailable(ServiceController _controller)
        {
            try
            {
                ServiceController[] controllers = ServiceController.GetServices();

                foreach (ServiceController c in controllers)
                {
                    if (c.ServiceName.Equals(_controller.ServiceName))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
            }
            return false;
        }

        private void SetConfigLocation()
        {
            String configLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MPExtended");
            hlStreamingConfigLocation.NavigateUri = new Uri(configLocation);
            tbStreamingConfigLocation.Text = Path.Combine(configLocation + "\\Streaming.xml");
        }

        private void LoadLogFiles(string fileName)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", fileName)))
            {
                try
                {
                    StreamReader re = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", fileName));
                    string input = null;
                    while ((input = re.ReadLine()) != null)
                    {
                        lvLogViewer.Items.Add(input);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionMessageBox(ex.Message);
                }

                //scroll to last item
                ScrollToLastItem(lvLogViewer);
            }
        }

        public void ScrollToLastItem(ListView lv)
        {

            lv.SelectedItem = lv.Items.GetItemAt(lv.Items.Count - 1);
            lv.ScrollIntoView(lv.SelectedItem);
            ListViewItem item = lv.ItemContainerGenerator.ContainerFromItem(lv.SelectedItem) as ListViewItem;
            if (item != null)
            {
                item.Focus();
            }
        }


        private void InitBackgroundWorker()
        {
            //task to watch the windows service
            if (mServiceController != null)
            {
                btnStartStopService.IsEnabled = true;
                mServiceWatcher = new DispatcherTimer();
                mServiceWatcher.Interval = TimeSpan.FromSeconds(2);
                mServiceWatcher.Tick += serviceWatcher_Tick;
                mServiceWatcher.Start();
            }
            else
            {
                lblServiceState.Content = "Not installed";
                btnStartStopService.IsEnabled = false;
            }

            //task to watch the streaming sessions, only started/stopped when tab is activated/deactivated

            mSessionWatcher = new DispatcherTimer();
            mSessionWatcher.Interval = TimeSpan.FromSeconds(2);
            mSessionWatcher.Tick += activeSessionWatcher_Tick;

            //task to update log listview, only started/stopped when tab is activated/deactivated
            mLogUpdater = new DispatcherTimer();
            mLogUpdater.Interval = TimeSpan.FromSeconds(2);
            mLogUpdater.Tick += logUpdater_Tick;
        }

        private void InitServiceConfiguration()
        {
            String user = null;
            String pass = null;
            Configuration.GetCredentials(out user, out pass, true);

            txtUsername.Text = user;
            txtUserPassword.Text = pass;

            txtServicePort.Text = Configuration.GetPort().ToString();
        }

        #region Logging Tab
        private bool mLogReaderRunning = false;
        private StreamReader mLogStreamReader = null;
        private long lastMaxOffset;
        private void InitLogTab(String _file)
        {
            mSelectedLog = _file;
            if (mSelectedLog != null)
            {
                lvLogViewer.Items.Clear();
                LoadLogFiles(mSelectedLog);

                String fileName = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", mSelectedLog);
                mLogStreamReader = new StreamReader(new FileStream(fileName,
                         FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                //start at the end of the file
                lastMaxOffset = mLogStreamReader.BaseStream.Length;

                mLogUpdater.Start();
            }
        }

        private void StopLogUpdater()
        {
            mLogUpdater.Stop();
            if (mLogStreamReader != null)
            {
                mLogStreamReader.Close();
            }
        }

        private delegate void AddLogDelegate(String _line);
        private void logUpdater_Tick(object sender, EventArgs e)
        {

            //if the file size has not changed, idle
            if (mLogStreamReader.BaseStream.Length == lastMaxOffset)
            {
                return;
            }

            //seek to the last max offset
            mLogStreamReader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

            //read out of the file until the EOF
            string line = "";
            while ((line = mLogStreamReader.ReadLine()) != null)
            {
                lvLogViewer.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (AddLogDelegate)delegate(String _line)
                {
                    lvLogViewer.Items.Add(_line);
                    if (cbLogScrollToEnd.IsChecked == true)
                    {
                        ScrollToLastItem(lvLogViewer);
                    }
                }, line);
            }
            //update the last max offset
            lastMaxOffset = mLogStreamReader.BaseStream.Position;
        }

        private void btnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".log";
            dlg.Filter = "Log file (.log)|*.log";
            if (dlg.ShowDialog() == true)
            {
                String logFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", mSelectedLog);

                string newFileName = dlg.FileName;

                File.Copy(logFile, newFileName);
            }
        }

        #endregion

        #region Streaming Sessions Tab
        private void DeInitStreamingTab()
        {
            mSessionWatcher.Stop();
        }

        private void InitStreamingTab()
        {
            mSessionWatcher.Start();
        }

        void activeSessionWatcher_Tick(object sender, EventArgs e)
        {
            try
            {
                List<WebStreamingSession> tmp = MPEServices.NetPipeWebStreamService.GetStreamingSessions();

                if (tmp != null)
                {
                    mStreamingSessions.UpdateStreamingList(tmp);
                    //lvActiveStreams.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    //{
                    //    lvActiveStreams.InvalidateProperty(ListView.ItemsSourceProperty);
                    //}));
                }
            }
            catch (CommunicationException)
            {
                mStreamingSessions.Clear();
                Log.Warn("No connection to service");
            }
        }

        #endregion

        #region Windows Service Functions
        private void serviceWatcher_Tick(object sender, EventArgs e)
        {
            try
            {
                mServiceController.Refresh();
                HandleServiceState(mServiceController.Status);
            }
            catch (Exception ex)
            {
                ExceptionMessageBox(ex.Message);
                mServiceWatcher.Stop();
            }
        }

        private void RestartService(int timeoutMilliseconds)
        {
            int millisec1 = Environment.TickCount;
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            mServiceController.Stop();
            mServiceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

            // count the rest of the timeout
            int millisec2 = Environment.TickCount;
            timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

            mServiceController.Start();
            mServiceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }

        private void HandleServiceState(ServiceControllerStatus _status)
        {
            switch (_status)
            {
                case ServiceControllerStatus.Stopped:
                    btnStartStopService.Content = "Start";
                    lblServiceState.Content = "Service Stopped";
                    lblServiceState.Foreground = System.Windows.Media.Brushes.Red;
                    break;
                case ServiceControllerStatus.Running:
                    btnStartStopService.Content = "Stop";
                    lblServiceState.Content = "Service Started";
                    lblServiceState.Foreground = System.Windows.Media.Brushes.Green;
                    break;
                case ServiceControllerStatus.StartPending:
                    btnStartStopService.Content = "Stop";
                    lblServiceState.Content = "Service Starting";
                    lblServiceState.Foreground = System.Windows.Media.Brushes.Teal;
                    break;
                default:
                    lblServiceState.Foreground = System.Windows.Media.Brushes.Teal;
                    lblServiceState.Content = "Service " + _status.ToString();
                    break;

            }
        }

        private void btnStartStopService_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin())
            {
                switch (mServiceController.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        UacServiceHelper.StartService();
                        break;
                    case ServiceControllerStatus.Running:
                        UacServiceHelper.StopService();
                        break;
                }
            }
            else
            {
                switch (mServiceController.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        mServiceController.Start();
                        break;
                    case ServiceControllerStatus.Running:
                        mServiceController.Stop();
                        break;

                }
            }
        }


        static internal bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion

        #region background worker, not used anymore
        /*
        void workerTVSeries_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = MPEServices.NetPipeMediaAccessService.GetTVShowCount().Count;
                IList<WebTVShowBasic> items = MPEServices.NetPipeMediaAccessService.GetAllTVShowsBasic(SortBy.Title, OrderBy.Asc);
                if (items != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        MessageBox.Show(count + " Tv Shows in Database", "Success");
                    }));
                }
            }
            catch (FaultException)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    MessageBox.Show("Couldn't open database! Check whether the paths are correct and the databases aren't empty");
                }));
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    ExceptionMessageBox(ex.Message);
                    Log.Error("Exception in workerTVSeries_DoWork", ex);
                }));
            }
        }

        void workerMusic_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                int count = MPEServices.NetPipeMediaAccessService.GetMusicAlbumCount().Count;
                IList<WebMusicAlbumBasic> items = MPEServices.NetPipeMediaAccessService.GetAllMusicAlbumsBasic();
                if (items != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        MessageBox.Show(count + " Albums in Database", "Success");
                    }));
                }
            }
            catch (FaultException)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    MessageBox.Show("Couldn't open database! Check whether the paths are correct and the databases aren't empty");
                }));
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    ExceptionMessageBox(ex.Message);
                    Log.Error("Exception in workerMusic_DoWork", ex);
                }));
            }
        }

        void workerServiceText_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                WebMediaServiceDescription functions = MPEServices.NetPipeMediaAccessService.GetServiceDescription();

                if (functions != null)
                {
                    String functionString = "Functions:";
                    //functionString += "\nSupports Videos: " + functions.SupportsVideos.ToString();
                    //functionString += "\nSupports Movies: " + functions.SupportsMovingPictures.ToString();
                    //functionString += "\nSupports Series: " + functions.SupportsTvSeries.ToString();
                    //functionString += "\nSupports Music: " + functions.SupportsMusic.ToString();
                    //functionString += "\nSupports Pictures: " + functions.SupportsPictures.ToString();

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
               {

                   MessageBox.Show(functionString, "Service connected!");
               }));
                }
            }
            catch (System.ServiceModel.FaultException ex)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    MessageBox.Show("Couldn't open database! Check whether the paths are correct and the databases aren't empty");
                }));

            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    ExceptionMessageBox(ex.Message);
                    Log.Error("Exception in workerServiceText_DoWork", ex);
                }));
            }


        }

        void workerMoPi_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = MPEServices.NetPipeMediaAccessService.GetMovieCount().Count;
                IList<WebMovieBasic> items = MPEServices.NetPipeMediaAccessService.GetMoviesBasicByRange(0, 1, SortBy.Title, OrderBy.Asc);
                if (items != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        MessageBox.Show(count + " Movies in Database", "Success");
                    }));

                }

            }
            catch (System.ServiceModel.FaultException ex)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    MessageBox.Show("Couldn't open database! Check whether the paths are correct and the databases aren't empty");
                }));

            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    ExceptionMessageBox(ex.Message);
                    Log.Error("Exception in workerMoPi_DoWork", ex);

                }));
            }
        }
         * 
         * */
        #endregion

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                if (tcMainTabs.SelectedItem.Equals(tiTroubleShooting))
                {
                    SetNetworkInterfaces();
                }

                if (tcMainTabs.SelectedItem.Equals(tiLogs))
                {
                    InitLogTab(mSelectedLog);
                }
                else
                {
                    StopLogUpdater();
                }

                if (tcMainTabs.SelectedItem.Equals(tiStreaming))
                {
                    InitStreamingTab();
                }
                else
                {
                    DeInitStreamingTab();
                }
            }
        }

        #region Troubleshooting Tab
        internal class MyNetworkAddress
        {
            internal NetworkInterface Interface { get; set; }
            internal IPAddressInformation Address { get; set; }

            public override string ToString()
            {
                return Address.Address + " (" + Interface.Name + ")";
            }
        }

        private void SetNetworkInterfaces()
        {
            cbNetworkInterfaces.Items.Clear();

            foreach (NetworkInterface n in NetworkInterface.GetAllNetworkInterfaces())
            {
                Log.Debug("Available Network Interface: {0}", n.Name);
                IPInterfaceProperties properties = n.GetIPProperties();

                foreach (IPAddressInformation unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Log.Debug("\tAvailable UniCast: {0}", unicast.Address);
                        cbNetworkInterfaces.Items.Add(new MyNetworkAddress() { Interface = n, Address = unicast });
                    }
                }
            }

            SetTestLinks("localhost", 4322);

        }

        private void cbNetworkInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyNetworkAddress addr = (MyNetworkAddress)cbNetworkInterfaces.SelectedItem;
            if (addr != null)
            {
                SetTestLinks(addr.Address.Address.ToString(), 4322);
            }
        }

        private void SetTestLinks(string _address, int _port)
        {
            String baseAdress = "http://{0}:{1}/MPExtended/{2}/json/{3}";

            String mediaAccessServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "MediaAccessService", "GetServiceDescription");
            hlTestLinkMediaAccessGeneral.NavigateUri = new Uri(mediaAccessServiceDescriptionUrl);
            tbTestLinkMediaAccessGeneral.Text = mediaAccessServiceDescriptionUrl;

            String tvAccessServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "TVAccessService", "GetServiceDescription");
            hlTestLinkTvAccessGeneral.NavigateUri = new Uri(tvAccessServiceDescriptionUrl);
            tbTestLinkTvAccessGeneral.Text = tvAccessServiceDescriptionUrl;

            String streamingServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "StreamingService", "GetServiceDescription");
            hlTestLinkStreamingGeneral.NavigateUri = new Uri(streamingServiceDescriptionUrl);
            tbTestLinkStreamingGeneral.Text = streamingServiceDescriptionUrl;
        }

        #endregion

        private void cmdUpdateConfig_Click(object sender, RoutedEventArgs e)
        {
            String user = null;
            String pass = null;
            Configuration.GetCredentials(out user, out pass, true);

            bool needsRestart = false;
            if (!txtUsername.Text.Equals(user) || !txtUserPassword.Text.Equals(pass))
            {
                if (!Configuration.SetCredentials(txtUsername.Text, txtUserPassword.Text))
                {
                    MessageBox.Show("Error updating config");
                    return;
                }
                
                needsRestart = true;
            }

            if (!txtServicePort.Text.Equals(Configuration.GetPort().ToString()))
            {
                if (!Configuration.SetPort(Int32.Parse(txtServicePort.Text)))
                {
                    MessageBox.Show("Error updating config");
                    return;
                }
            }


            if (needsRestart)
            {
                MessageBoxResult res = MessageBox.Show("Successfully updated config, restart service now?", "Config", MessageBoxButton.YesNo);

                if (res == MessageBoxResult.Yes)
                {
                    if (!IsAdmin())
                    {
                        UacServiceHelper.RestartService();
                    }
                    else
                    {
                        RestartService(5000);
                    }
                }
            }
        }

        private static void ExceptionMessageBox(string exMessage)
        {
            MessageBox.Show("An unexpected error occured. Please file a bugreport with the service's log files attached", exMessage);
        }

        private void cbPluginConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<String, PluginConfigItem> selected = (Dictionary<String, PluginConfigItem>)cbPluginConfigs.SelectedValue;
            sectionPluginSettings.SetPluginConfig(mConfigFile, selected);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }


        #region Barcode
        /// <summary>
        /// Save the generated barcode as image file to harddisk
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.Filter = "JPEG Image|*.jpg";
            diag.Title = "Save Barcode as Image File";
            if (diag.ShowDialog() == true)
            {
                ((BitmapSource)imgQRCode.Source).ToWinFormsBitmap().Save(diag.FileName);
                //imgQRCode..Image.Save(diag.FileName);
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            GenerateBarcode(checkBox1.IsChecked == true);
        }

        /// <summary>
        /// Generate a QR Barcode with the server information
        /// </summary>
        private void GenerateBarcode(bool _includeAuth)
        {
            try
            {
                ServerDescription desc = new ServerDescription();
                desc.GeneratorApp = "ServiceConfigurator";
                desc.ServiceType = "Client";
                
                desc.Port = Int32.Parse(txtServicePort.Text);
                desc.Name = txtServiceName.Text;
                desc.HardwareAddresses = GetHardwareAddresses();
                desc.Hostname = GetServiceName();

                IPHostEntry host;
                String localIP = "?";
                StringBuilder localIPs = new StringBuilder();
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Single address field
                        localIP = ip.ToString();

                        // Multiple addresses field
                        if (localIPs.Length > 0)
                        {
                            localIPs.Append(";");
                        }

                        localIPs.Append(ip.ToString());
                    }
                }

                desc.Addresses = (localIPs.Length > 0) ? localIPs.ToString() : "?";

                if (_includeAuth)
                {
                    desc.User = txtUsername.Text;
                    desc.Password = txtUserPassword.Text;
                    desc.AuthOptions = 1;//username/password
                }

                Bitmap bm = QRCodeGenerator.Generate(desc.ToJSON());
                imgQRCode.Source = bm.ToWpfBitmap();
            }
            catch (Exception ex)
            {
                Log.Error("[WifiRemote Setup] Error generating barcode: {0}", ex.Message);
            }
        }


        public static String GetHardwareAddresses()
        {
            StringBuilder hardwareAddresses = new StringBuilder();
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up)
                    {
                        String hardwareAddress = adapter.GetPhysicalAddress().ToString();
                        if (!hardwareAddress.Equals(String.Empty) && hardwareAddress.Length == 12)
                        {
                            if (hardwareAddresses.Length > 0)
                            {
                                hardwareAddresses.Append(";");
                            }

                            hardwareAddresses.Append(hardwareAddress);
                        }
                    }
                }
            }
            catch (NetworkInformationException e)
            {
                Log.Warn("Could not get hardware address", e);
            }

            return hardwareAddresses.ToString();
        }

        /// <summary>
        /// Get the machine name or a fallback
        /// </summary>
        /// <returns></returns>
        public static string GetServiceName()
        {
            try
            {
                return System.Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                return "MP-Extended Service";
            }
        }
        #endregion

        private void miKickUserSession_Click(object sender, RoutedEventArgs e)
        {
            WpfStreamingSession session = (WpfStreamingSession)lvActiveStreams.SelectedItem;
            bool success = MPEServices.NetPipeWebStreamService.FinishStream(session.Identifier);
        }







        
    }
}
