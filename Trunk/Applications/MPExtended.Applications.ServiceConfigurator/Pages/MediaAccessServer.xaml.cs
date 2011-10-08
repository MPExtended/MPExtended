#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using System.Xml.Linq;
using MPExtended.Services.MediaAccessService;
using System.Diagnostics;
using System.Security.Principal;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for MediaAccessServer.xaml
    /// </summary>
    public partial class MediaAccessServer : Page
    {
        private ServiceController mServiceController;
        private DispatcherTimer mServiceWatcher;
        private List<WebStreamingSession> mStreamingSessions = new List<WebStreamingSession>();
        private static ITVAccessService _tvService;
        private static IMediaAccessService _mediaService;
        private static IStreamingService _streamingService;
        private static IWebStreamingService _webStreamingService;
        private Timer activeSessionTimer = new Timer();
        private InstallationType mInstallationType = Installation.GetInstallationType();

        Dictionary<String, Dictionary<String, PluginConfigItem>> PluginConfigurations { get; set; }

        BackgroundWorker workerActiveSessions = new BackgroundWorker();
        BackgroundWorker workerMusic = new BackgroundWorker();
        BackgroundWorker workerTVSeries = new BackgroundWorker();
        BackgroundWorker workerVideos = new BackgroundWorker();
        BackgroundWorker workerMoPi = new BackgroundWorker();
        BackgroundWorker workerServiceText = new BackgroundWorker();

        public MediaAccessServer()
        {
            InitializeComponent();

            try
            {
                mServiceController = new ServiceController("MPExtended Service");
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("MPExtended service is not installed! Please install the latest version.");
                return;
            }

            switch (mInstallationType)
            {
                case InstallationType.Singleseat:
                    LoadLogFiles("SingleSeat");
                    break;
                case InstallationType.Server:
                    LoadLogFiles("Server");
                    break;
                case InstallationType.Client:
                    LoadLogFiles("Client");
                    break;
            }


            //Load the MediaAccess.xml configuration file
            PluginConfigurations = new Dictionary<String, Dictionary<String, PluginConfigItem>>();
            try
            {
                var config = XElement.Load(Configuration.GetPath("MediaAccess.xml")).Element("pluginConfiguration").Elements("plugin");

                foreach (var p in config)
                {
                    Dictionary<String, PluginConfigItem> props = new Dictionary<String, PluginConfigItem>();
                    foreach (var p2 in p.Descendants())
                    {
                        props.Add(p2.Name.LocalName, new PluginConfigItem(p2));
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

            if (mServiceController != null)
            {
                mServiceWatcher = new DispatcherTimer();
                mServiceWatcher.Interval = TimeSpan.FromSeconds(2);
                mServiceWatcher.Tick += timer1_Tick;
                mServiceWatcher.Start();
            }

            activeSessionTimer.Elapsed += new ElapsedEventHandler(activeSessionTimer_Elapsed);
            activeSessionTimer.Interval = 18000;
            activeSessionTimer.Enabled = true;
            activeSessionTimer.AutoReset = true;
            InitBackgroundWorker();
        }

        private bool isServiceAvailable(ServiceController _controller)
        {
            ServiceController[] controllers = ServiceController.GetServices();

            foreach (ServiceController c in controllers)
            {
                if (c.ServiceName.Equals(_controller.ServiceName))
                {
                    return true;
                }
            }
            return false;
        }

        private void LoadLogFiles(string fileName)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\{0}.log", fileName)))
            {
                try
                {
                    StreamReader re = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\{0}.log", fileName));
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
            }
        }

        private void InitBackgroundWorker()
        {
            workerMoPi.DoWork += new DoWorkEventHandler(workerMoPi_DoWork);
            workerServiceText.DoWork += new DoWorkEventHandler(workerServiceText_DoWork);
            workerMusic.DoWork += new DoWorkEventHandler(workerMusic_DoWork);
            workerTVSeries.DoWork += new DoWorkEventHandler(workerTVSeries_DoWork);
            workerActiveSessions.DoWork += new DoWorkEventHandler(workerActiveSessions_DoWork);
        }


        void activeSessionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!workerActiveSessions.IsBusy)
            {
                workerActiveSessions.RunWorkerAsync();
            }
        }

        void workerActiveSessions_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<WebStreamingSession> tmp = MPEServices.NetPipeWebStreamService.GetStreamingSessions();
                if (tmp != null)
                {
                    lvActiveStreams.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        lvActiveStreams.ItemsSource = MPEServices.NetPipeWebStreamService.GetStreamingSessions();
                    }));
                }
            }
            catch (EndpointNotFoundException)
            {
                Log.Warn("No connection to service");
            }
        }


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



        private void InitWebservice()
        {
            String user = null;
            String pass = null;
            Configuration.GetCredentials(out user, out pass, true);

        }



        private void timer1_Tick(object sender, EventArgs e)
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }

        private void HandleServiceState(ServiceControllerStatus _status)
        {
            switch (_status)
            {
                case ServiceControllerStatus.Stopped:
                    btnStartStopService.Content = "Start";
                    lblServiceState.Content = "Service Stopped";
                    break;
                case ServiceControllerStatus.Running:
                    btnStartStopService.Content = "Stop";
                    lblServiceState.Content = "Service Started";
                    break;
                case ServiceControllerStatus.StartPending:
                    btnStartStopService.Content = "Stop";
                    lblServiceState.Content = "Service Starting";
                    break;
                default:
                    lblServiceState.Content = "Service " + _status.ToString();
                    break;

            }
        }

        private void btnStartStopService_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdmin())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "MPExtended.Applications.UacServiceHandler.exe";
                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                
                switch (mServiceController.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        info.Arguments = "start";
                        break;
                    case ServiceControllerStatus.Running:
                        info.Arguments = "stop";
                        break;

                }

                if (Process.Start(info) == null)
                {
                    // The user didn't accept the UAC prompt.
                    MessageBox.Show("This action needs administrative rights");
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

     
        private void btnBrowseVideos_Click(object sender, RoutedEventArgs e)
        {
            BrowseForDbLocation(DatabaseType.Videos);
        }

        private void btnBrowseMopi_Click(object sender, RoutedEventArgs e)
        {
            BrowseForDbLocation(DatabaseType.Mopi);
        }

        private void btnBrowseTvSeries_Click(object sender, RoutedEventArgs e)
        {
            BrowseForDbLocation(DatabaseType.TvSeries);
        }

        private void btnBrowseMusic_Click(object sender, RoutedEventArgs e)
        {
            BrowseForDbLocation(DatabaseType.Music);
        }

        private void btnBrowsePictures_Click(object sender, RoutedEventArgs e)
        {
            BrowseForDbLocation(DatabaseType.Pictures);
        }

        private void btnTestVideos_Click(object sender, RoutedEventArgs e)
        {
            workerVideos.RunWorkerAsync();
        }

        private void btnTestMopi_Click(object sender, RoutedEventArgs e)
        {
            workerMoPi.RunWorkerAsync();
        }

        private void btnTestTvSeries_Click(object sender, RoutedEventArgs e)
        {
            workerTVSeries.RunWorkerAsync();
        }

        private void btnTestMusic_Click(object sender, RoutedEventArgs e)
        {
            workerMusic.RunWorkerAsync();
        }

        private void btnTestPictures_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented yet", "Error");
        }

        private enum DatabaseType { Pictures, Videos, Music, Mopi, TvSeries }

        private void BrowseForDbLocation(DatabaseType _db)
        {
            // Open document
            /*string oldFolder = null;

            switch (_db)
            {
                case DatabaseType.Pictures:
                    oldFolder = Configuration.GetMPDbLocations().Pictures;
                    break;
                case DatabaseType.Videos:
                    oldFolder = Configuration.GetMPDbLocations().Videos;
                    break;
                case DatabaseType.Music:
                    oldFolder = Configuration.GetMPDbLocations().Music;
                    break;
                case DatabaseType.Mopi:
                    oldFolder = Configuration.GetMPDbLocations().MovingPictures;
                    break;
                case DatabaseType.TvSeries:
                    oldFolder = Configuration.GetMPDbLocations().TvSeries;
                    break;
            }

            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            if (oldFolder != null)
            {
                dlg.InitialDirectory = new FileInfo(oldFolder).Directory.FullName;
            }

            dlg.DefaultExt = ".db"; // Default file extension
            dlg.Filter = "Database files (.db3)|*.db3|All files (.*)|*.*"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;

                switch (_db)
                {
                    case DatabaseType.Pictures:
                        txtDbLocationPictures.Text = filename;
                        Configuration.ChangeDbLocation("pictures", filename);
                        break;
                    case DatabaseType.Videos:
                        txtDbLocationVideos.Text = filename;
                        Configuration.ChangeDbLocation("videos", filename);
                        break;
                    case DatabaseType.Music:
                        txtDbLocationMusic.Text = filename;
                        Configuration.ChangeDbLocation("music", filename);
                        break;
                    case DatabaseType.Mopi:
                        txtDbLocationMopi.Text = filename;
                        Configuration.ChangeDbLocation("movingpictures", filename);
                        break;
                    case DatabaseType.TvSeries:
                        txtDbLocationTvSeries.Text = filename;
                        Configuration.ChangeDbLocation("tvseries", filename);
                        break;
                }
            }
            */

        }

        private void cmdUpdateConfig_Click(object sender, RoutedEventArgs e)
        {
            if (Configuration.SetCredentials(txtUsername.Text, txtUserPassword.Text))
            {
                MessageBox.Show("Successfully updated config");
            }
            else
            {
                MessageBox.Show("Error updating config");
            }
        }

        private void btnTestWebService_Click(object sender, RoutedEventArgs e)
        {
            workerServiceText.RunWorkerAsync();
        }

        private static void ExceptionMessageBox(string exMessage)
        {
            MessageBox.Show("An unexpected error occured please file an issue on mpextended.codeplex.com with the service's log files attached", exMessage);
        }

        private void cbPluginConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<String, PluginConfigItem> selected = (Dictionary<String, PluginConfigItem>)cbPluginConfigs.SelectedValue;
            sectionPluginSettings.SetPluginConfig(selected);
        }
    }
}
