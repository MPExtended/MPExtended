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
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for MediaAccessServer.xaml
    /// </summary>
    public partial class ServiceConfiguration : Page
    {
        private ServiceController mServiceController;
        private DispatcherTimer mServiceWatcher;
        private int lastTabIndex = -1;

        public ServiceConfiguration()
        {
            InitializeComponent();

            // initialize server controller
            try
            {
                mServiceController = new ServiceController("MPExtended Service");
                HandleServiceState(mServiceController.Status);
            }
            catch (InvalidOperationException)
            {
                mServiceController = null;
                Log.Error("MPExtended Service not installed");
                MessageBox.Show("MPExtended Service not installed. Please make sure your installation is correct.", "MPExtended", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // initialize watcher
            InitBackgroundWorker();

            // hide tabs not applicable for current situation
            if (!Installation.IsServiceInstalled(MPExtendedService.MediaAccessService))
            {
                tcMainTabs.Items.Remove(tiPlugin);
                tcMainTabs.Items.Remove(tiSocial);
            }
            if (!Installation.IsServiceInstalled(MPExtendedService.StreamingService))
            {
                tcMainTabs.Items.Remove(tiStreaming);
                tcMainTabs.Items.Remove(tiSocial);
            }
        }

        /// <summary>
        /// Loads the pages into the content
        /// </summary>
        private void tcMainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lastTabIndex != tcMainTabs.SelectedIndex)
            {
                TabItem item = tcMainTabs.SelectedItem as TabItem;

                Frame f = new Frame();
                f.Source = new Uri((string)item.Tag, UriKind.Relative);
                item.Content = f;
                lastTabIndex = tcMainTabs.SelectedIndex;
            }
        }

        /// <summary>
        /// Initialize the service watcher.
        /// </summary>
        private void InitBackgroundWorker()
        {
            // Task to watch the windows service
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
        }

        private void serviceWatcher_Tick(object sender, EventArgs e)
        {
            try
            {
                mServiceController.Refresh();
                HandleServiceState(mServiceController.Status);
            }
            catch (Exception ex)
            {
                ErrorHandling.ShowError(ex);
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
                    lblServiceState.Content = "Service stopped";
                    lblServiceState.Foreground = Brushes.Red;
                    break;
                case ServiceControllerStatus.Running:
                    btnStartStopService.Content = "Stop";
                    lblServiceState.Content = "Service started";
                    lblServiceState.Foreground = Brushes.Green;
                    break;
                case ServiceControllerStatus.StartPending:
                    btnStartStopService.Content = "Stop";
                    lblServiceState.Content = "Service starting";
                    lblServiceState.Foreground = Brushes.Teal;
                    break;
                default:
                    lblServiceState.Foreground = Brushes.Teal;
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

        private bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
