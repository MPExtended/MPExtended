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
using System.ServiceProcess;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;
using System.ServiceModel;
using MPExtended.Libraries.ServiceLib;

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
        private System.Timers.Timer activeSessionTimer = new System.Timers.Timer();

        private string ServiceName { get { return checkInstalledServices(); } }

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
                switch (ServiceName)
                {
                    case "MPExtendedSingleSeat":
                        mServiceController = new ServiceController("MPExtended SingleSeat Service");
                        LoadSingleSeatLogFiles();
                        break;
                    case "MPExtendedMultiSeatServer":
                        mServiceController = new ServiceController("MPExtended Server Service");
                        LoadServerLogFiles();
                        break;
                    case "MPExtendedMultiSeatClient":
                        mServiceController = new ServiceController("MPExtended Client Service");
                        LoadClientLogFiles();
                        break;

                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("MPExtended webservice is not installed! Please install the latest version");
            }
            if (mServiceController != null)
            {
                mServiceWatcher = new DispatcherTimer();
                mServiceWatcher.Interval = TimeSpan.FromSeconds(2);
                mServiceWatcher.Tick += timer1_Tick;
                mServiceWatcher.Start();
            }
            activeSessionTimer.Elapsed += new System.Timers.ElapsedEventHandler(activeSessionTimer_Elapsed);
            activeSessionTimer.Interval = 18000;
            activeSessionTimer.Enabled = true;
            activeSessionTimer.AutoReset = true;       
            InitBackgroundWorker();

        }

        void activeSessionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            InitActiveSessionWatcher();
        }
        private void LoadClientLogFiles()
        {
            LoadLogFiles("Client");
        }
        private void LoadServerLogFiles()
        {
            LoadLogFiles("Server");
        }
        private void LoadSingleSeatLogFiles()
        {
            //LoadLogFiles("SingleSeat");
        }
        private void LoadLogFiles(string fileName)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\{0].log",fileName)))
            {
                try
                {
                    StreamReader re = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\{0].log", fileName));
                    string input = null;
                    while ((input = re.ReadLine()) != null)
                    {
                        lvLogViewer.Items.Add(input);
                    }
                }
                catch (Exception ex)
                { ExceptionMessageBox(ex.Message); }
            }

        }

        private void InitBackgroundWorker()
        {
            workerVideos.DoWork += new DoWorkEventHandler(workerVideos_DoWork);
            workerMoPi.DoWork += new DoWorkEventHandler(workerMoPi_DoWork);
            workerServiceText.DoWork += new DoWorkEventHandler(workerServiceText_DoWork);
            workerMusic.DoWork += new DoWorkEventHandler(workerMusic_DoWork);
            workerTVSeries.DoWork += new DoWorkEventHandler(workerTVSeries_DoWork);
            workerActiveSessions.DoWork += new DoWorkEventHandler(workerActiveSessions_DoWork);
        }

        void workerActiveSessions_DoWork(object sender, DoWorkEventArgs e)
        {
            List<WebStreamingSession> tmp = WebStreamingService.GetStreamingSessions().ToList();
            if (tmp != null)
            {
                lvActiveStreams.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                {
                    lvActiveStreams.ItemsSource = WebStreamingService.GetStreamingSessions().ToList();
                }));
            }
        }

        private void InitActiveSessionWatcher()
        {
            if (!workerActiveSessions.IsBusy)
            {
                workerActiveSessions.RunWorkerAsync();
            }

        }

        void workerTVSeries_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = MediaService.GetSeriesCount();
                List<WebSeries> items = MediaService.GetSeries(0, 1, SortBy.Name, OrderBy.Asc);
                if (items != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        MessageBox.Show(count + " Tv Shows in Database", "Success");
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
                    Log.Error("Exception in workerTVSeries_DoWork", ex);
                }));
            }

        }

        void workerMusic_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                int count = MediaService.GetAlbumsCount();
                List<WebMusicAlbum> items = MediaService.GetAlbums(0, 1, SortBy.Name, OrderBy.Asc);
                if (items != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
                    {
                        MessageBox.Show(count + " Albums in Database", "Success");
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
                    Log.Error("Exception in workerMusic_DoWork", ex);
                }));
            }
        }

        void workerServiceText_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                WebServiceDescription functions = MediaService.GetServiceDescription();

                if (functions != null)
                {
                    String functionString = "Functions:";
                    functionString += "\nSupports Videos: " + functions.SupportsVideos.ToString();
                    functionString += "\nSupports Movies: " + functions.SupportsMovingPictures.ToString();
                    functionString += "\nSupports Series: " + functions.SupportsTvSeries.ToString();
                    functionString += "\nSupports Music: " + functions.SupportsMusic.ToString();
                    functionString += "\nSupports Pictures: " + functions.SupportsPictures.ToString();

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
                int count = MediaService.GetMovieCount();
                List<WebMovie> items = MediaService.GetMovies(0, 1, SortBy.Name, OrderBy.Asc);
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

        void workerVideos_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = MediaService.GetVideosCount();
                List<WebMovie> items = MediaService.GetVideos(0, 1, SortBy.Name, OrderBy.Asc);
                if (items != null)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
{
    MessageBox.Show(count + " Videos in Database", "Success");
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
                   Log.Error("Exception in workerVideos_DoWork", ex);
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
            if (TabDatabases.IsSelected)
            {
                DBLocations dbLocations = Configuration.GetMPDbLocations();
                txtDbLocationMopi.Text = dbLocations.MovingPictures;
                txtDbLocationMopi.CaretIndex = txtDbLocationMopi.Text.Length;

                txtDbLocationMusic.Text = dbLocations.Music;
                txtDbLocationMusic.CaretIndex = txtDbLocationMusic.Text.Length;

                txtDbLocationVideos.Text = dbLocations.Videos;
                txtDbLocationVideos.CaretIndex = txtDbLocationVideos.Text.Length;

                txtDbLocationTvSeries.Text = dbLocations.TvSeries;
                txtDbLocationTvSeries.CaretIndex = txtDbLocationTvSeries.Text.Length;

                txtDbLocationPictures.Text = dbLocations.Pictures;
                txtDbLocationPictures.CaretIndex = txtDbLocationPictures.Text.Length;
            }

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
            string oldFolder = null;

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
        private string checkInstalledServices()
        {
            if (File.Exists(Configuration.GetPath("Streaming.xml")) && File.Exists(Configuration.GetPath("TVAccess.xml")) && File.Exists(Configuration.GetPath("MediaAccess.xml")))
            {
                return "MPExtendedSingleSeat";
            }
            if (File.Exists(Configuration.GetPath("Streaming.xml")) && File.Exists(Configuration.GetPath("MediaAccess.xml")))
            {
                return "MPExtendedMultiSeatClient";
            }
            if (File.Exists(Configuration.GetPath("Streaming.xml")) && File.Exists(Configuration.GetPath("MediaAccess.xml")))
            {
                return "MPExtendedMultiSeatClient";
            }
            return "";
        }

        public static ITVAccessService TVService
        {
            get
            {
                if (_tvService == null || ((ICommunicationObject)_tvService).State == CommunicationState.Faulted)
                {
                    try
                    {

                        _tvService = ChannelFactory<ITVAccessService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/StreamingService/TVAccessService"));
                    }
                    catch (EndpointNotFoundException ex)
                    { }
                    catch (Exception ex)
                    { ExceptionMessageBox(ex.Message); }
                }

                return _tvService;
            }
        }
        public static IStreamingService StreamingService
        {
            get
            {
                if (_streamingService == null || ((ICommunicationObject)_streamingService).State == CommunicationState.Faulted)
                {
                    try
                    {
                        _streamingService = ChannelFactory<IStreamingService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/StreamingService"));
                    }
                    catch (EndpointNotFoundException ex)
                    { }
                    catch (Exception ex)
                    { ExceptionMessageBox(ex.Message); }
                }

                return _streamingService;
            }
        }
        public static IWebStreamingService WebStreamingService
        {
            get
            {
                if (_webStreamingService == null || ((ICommunicationObject)_webStreamingService).State == CommunicationState.Faulted)
                {
                    try
                    {
                        _webStreamingService = ChannelFactory<IWebStreamingService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/StreamingService"));
                    }
                    catch (EndpointNotFoundException ex)
                    { }
                    catch (Exception ex)
                    { ExceptionMessageBox(ex.Message); }
                }

                return _webStreamingService;
            }
        }
        public static IMediaAccessService MediaService
        {
            get
            {
                if (_mediaService == null || ((ICommunicationObject)_mediaService).State == CommunicationState.Faulted)
                {
                    try
                    {
                        _mediaService = ChannelFactory<IMediaAccessService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/MediaAccessService"));
                    }
                    catch (EndpointNotFoundException ex)
                    { }
                    catch (Exception ex)
                    {
                        ExceptionMessageBox(ex.Message);
                    }
                }

                return _mediaService;
            }
        }

        private static void ExceptionMessageBox(string exMessage)
        {

            MessageBox.Show("An unexpected error occured please file an issue on mpextended.codeplex.com with the service's log files attached", exMessage);

        }
    }
}
