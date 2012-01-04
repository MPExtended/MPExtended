#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.ServiceModel;
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
using MPExtended.Libraries.General;
using MPExtended.Services.UserSessionService;
using MPExtended.Services.UserSessionService.Interfaces;
using MPExtended.Applications.ServiceConfigurator.Code;

namespace MPExtended.Applications.ServiceConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool mIsAppExiting = false;
        private ServiceHost ussHost;
        private ServiceHost privateHost;
        private UserSessionService userSessionService;

        public MainWindow()
        {
            InitializeComponent();
            userSessionService = new UserSessionService();

            try
            {
                Log.Debug("MPExtended.Applications.ServiceConfigurator starting...");
                ussHost = new ServiceHost(typeof(MPExtended.Services.UserSessionService.UserSessionService));
                privateHost = new ServiceHost(typeof(MPExtended.Applications.ServiceConfigurator.Code.PrivateUserSessionService));

                Log.Debug("Opening ServiceHost...");
                ussHost.Open();
                privateHost.Open();

                Log.Info("UserSessionService started...");
            }
            catch (AddressAlreadyInUseException)
            {
                Log.Info("Address for UserSessionService is already in use");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start service", ex);
            }

            try
            {
                MainFrame.Navigate(new Pages.ServiceConfiguration());
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open configurator", ex);
                ErrorHandling.ShowError(ex);
            }

            if (StartupArguments.RunAsTrayApp && !StartupArguments.OpenOnStart)
                Hide();

            HandleMpState(userSessionService.IsMediaPortalRunning().Result);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // When the application is closed, check whether the application is exiting from menu or forms close button
            if (!mIsAppExiting && StartupArguments.RunAsTrayApp)
            {
                // If the form close button is triggered, cancel the event and hide the form,
                // then show the notification ballon tip.
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                ussHost.Close();
                privateHost.Close();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            mIsAppExiting = true;
            this.Close();
        }

        private void MenuStartCloseMp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isMpRunning = userSessionService.IsMediaPortalRunning().Result;
                if (!isMpRunning)
                {
                    userSessionService.StartMediaPortal();
                    HandleMpState(true);
                }
                else
                {
                    userSessionService.CloseMediaPortal();
                    HandleMpState(false);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Exception while trying to start/stop MediaPortal", ex);
            }
        }

        private void HandleMpState(bool _running)
        {
            if (_running)
            {
                MenuStartCloseMp.Header = "Close MediaPortal";
            }
            else
            {
                MenuStartCloseMp.Header = "Start MediaPortal";
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            HandleMpState(userSessionService.IsMediaPortalRunning().Result);
        }

        private void MenuOpenConfigurator_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void TaskbarIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void MenuPowermodeLogoff_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.LogOff);
        }

        private void MenuPowermodeSuspend_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.Suspend);
        }

        private void MenuPowermodeHibernate_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.Hibernate);
        }

        private void MenuPowermodeReboot_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.Reboot);
        }

        private void MenuPowermodeShutdown_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.PowerOff);
        }

        private void MenuPowermodeLock_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.Lock);
        }

        private void MenuPowermodeMonitorOff_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.ScreenOff);
        }

        private void MenuPowermodeScreensaverOn_Click(object sender, RoutedEventArgs e)
        {
            userSessionService.SetPowerMode(WebPowerMode.Screensaver);
        }
    }
}
