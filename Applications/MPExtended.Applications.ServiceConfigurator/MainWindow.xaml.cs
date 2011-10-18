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

namespace MPExtended.Applications.ServiceConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const bool ONLY_CONFIGURATOR = true;

        private bool mIsAppExiting = false;
        private ServiceHost mUserSessionHost;
        private IUserSessionService mUserSessionService;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Log.Debug("MPExtended.Applications.ServiceConfigurator starting...");
                mUserSessionHost = new ServiceHost(typeof(MPExtended.Services.UserSessionService.UserSessionService));
                Log.Debug("Opening ServiceHost...");

                mUserSessionHost.Open();
                Log.Debug("Host opened");
                Log.Info("UserSessionService started...");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start service", ex);
            }

            mUserSessionService = new UserSessionService();

            try
            {
                MainFrame.Navigate(new Pages.MediaAccessServer());
            }
            catch (Exception ex)
            {
                Log.Error("Failed to open configurator", ex);
                MessageBox.Show(ex.ToString());
            }

            if (!ONLY_CONFIGURATOR)
                Hide();

            HandleMpState(mUserSessionService.IsMediaPortalRunning().Status);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // When the application is closed, check whether the application is
            // exiting from menu or forms close button
            if (!mIsAppExiting && !ONLY_CONFIGURATOR)
            {
                // if the forms close button is triggered, cancel the event and hide the form
                // then show the notification ballon tip
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                mUserSessionHost.Close();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            mIsAppExiting = true;
            this.Close();
        }

        private void MenuStartCloseMp_Click(object sender, RoutedEventArgs e)
        {
            bool isMpRunning = mUserSessionService.IsMediaPortalRunning().Status;
            if (!isMpRunning)
            {
                mUserSessionService.StartMediaPortal();
                HandleMpState(true);
            }
            else
            {
                mUserSessionService.CloseMediaPortal();
                HandleMpState(false);
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
            HandleMpState(mUserSessionService.IsMediaPortalRunning().Status);
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
            mUserSessionService.SetPowerMode(WebPowerMode.LogOff);
        }

        private void MenuPowermodeSuspend_Click(object sender, RoutedEventArgs e)
        {
            mUserSessionService.SetPowerMode(WebPowerMode.Suspend);
        }

        private void MenuPowermodeHibernate_Click(object sender, RoutedEventArgs e)
        {
            mUserSessionService.SetPowerMode(WebPowerMode.Hibernate);
        }

        private void MenuPowermodeReboot_Click(object sender, RoutedEventArgs e)
        {
            mUserSessionService.SetPowerMode(WebPowerMode.Reboot);
        }

        private void MenuPowermodeShutdown_Click(object sender, RoutedEventArgs e)
        {
            mUserSessionService.SetPowerMode(WebPowerMode.PowerOff);
        }
    }
}
