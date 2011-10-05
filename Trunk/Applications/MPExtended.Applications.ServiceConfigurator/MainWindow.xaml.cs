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
using MPExtended.Libraries.General;
using System.ServiceModel;
using MPExtended.Services.UserSessionService;

namespace MPExtended.Applications.ServiceConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool mIsAppExiting = false;
        private ServiceHost mUserSessionHost;
        private UserSessionService mUserSessionService;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Log.Debug("MPExtended.ServiceHosts.Console.Client starting...");

                mUserSessionHost = new ServiceHost(typeof(MPExtended.Services.UserSessionService.UserSessionService));
                Log.Debug("Opening ServiceHost...");

                mUserSessionHost.Open();


                Log.Debug("Host opened");

                Log.Info("UserSessionServiceBehavior started...");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            mUserSessionService = new UserSessionService();

            try
            {
                MainFrame.Navigate(new Pages.MediaAccessServer());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //When the application is closed, check wether the application is
            //exiting from menu or forms close button
            if (!mIsAppExiting)
            {
                //if the forms close button is triggered, cancel the event and hide the form
                //then show the notification ballon tip
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                mUserSessionHost.Close();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            mIsAppExiting = true;
            this.Close();
        }

        private void MenuStartCloseMp_Click(object sender, RoutedEventArgs e)
        {
            bool isMpRunning = mUserSessionService.IsMediaPortalRunning();
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
            HandleMpState(mUserSessionService.IsMediaPortalRunning());
        }

        private void MenuPowermodeLogoff_Click(object sender, RoutedEventArgs e)
        {
            mUserSessionService.SetPowerMode(WebPowerModes.LogOff);
        }

        private void MenuPowermodeSuspend_Click(object sender, RoutedEventArgs e)
        {
            mUserSessionService.SetPowerMode(WebPowerModes.Suspend);
        }

        private void TaskbarIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            this.Show();
        }




    }
}
