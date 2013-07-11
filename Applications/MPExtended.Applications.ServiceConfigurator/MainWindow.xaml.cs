#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Applications.ServiceConfigurator.Pages;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Composition;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Services.UserSessionService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool mIsAppExiting = false;
        private ServiceControlInterface sci;
        private DispatcherTimer mServiceWatcher;
        private int lastTabIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            Log.Debug("MPExtended configurator version {0} starting...", VersionUtil.GetFullVersionString());
            SetupUSS();

            if (StartupArguments.RunAsTrayApp && !StartupArguments.OpenOnStart)
                Hide();

            HandleMediaPortalState(UserServices.USS.IsMediaPortalRunning());

            // service control interface
            sci = new ServiceControlInterface("MPExtended Service", lblServiceState, btnStartStopService);
            if (!sci.IsServiceAvailable && Installation.GetFileLayoutType() == FileLayoutType.Installed)
            {
                Log.Error("MPExtended Service not installed");
                MessageBox.Show(UI.ServiceNotInstalledPopup, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (sci.IsServiceAvailable)
            {
                sci.StartServiceWatcher();
            }

            // hide tabs and context menu items not applicable for current situation
            if (!Installation.IsServiceInstalled("MediaAccessService"))
            {
                tcMainTabs.Items.Remove(tiPlugin);
                tcMainTabs.Items.Remove(tiSocial);
            }
            if (!Installation.IsServiceInstalled("StreamingService"))
            {
                tcMainTabs.Items.Remove(tiStreaming);
                tcMainTabs.Items.Remove(tiSocial);
            }
            if (!Installation.IsProductInstalled(MPExtendedProduct.WebMediaPortal))
            {
                tcMainTabs.Items.Remove(tiWebMediaPortal);
                taskbarItemContextMenu.Items.Remove(MenuOpenWebMP);
            }

            // initialize some tabs
            Pages.TabConfiguration.StartLoadingTranslations();
        }

        private void SetupUSS()
        {
            if (Installation.GetFileLayoutType() == FileLayoutType.Installed)
            {
                AssemblyLoader.Install();
                AssemblyLoader.AddSearchDirectory(Path.Combine(Installation.GetInstallDirectory(), "Plugins", "Services"));
            }
            UserServices.Setup(true);
		}

        private void CloseTab()
        {
            try
            {
                if (lastTabIndex >= 0 && ((tcMainTabs.Items[lastTabIndex] as TabItem).Content as Frame).Content is ITabCloseCallback)
                {
                    (((tcMainTabs.Items[lastTabIndex] as TabItem).Content as Frame).Content as ITabCloseCallback).TabClosed();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to handle TabClosed() callback when leaving tab", ex);
                ErrorHandling.OnlyShowError(ex);
            }
        }

        /// <summary>
        /// Loads the pages into the content
        /// </summary>
        private void tcMainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lastTabIndex != tcMainTabs.SelectedIndex)
                {
                    CloseTab();
                    TabItem item = tcMainTabs.SelectedItem as TabItem;
                    Frame f = new Frame();
                    f.Source = new Uri((string)item.Tag, UriKind.Relative);
                    item.Content = f;
                    lastTabIndex = tcMainTabs.SelectedIndex;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to change tab", ex);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            CommonEventHandlers.NavigateHyperlink(sender, e);
        }

        private void btnStartStopService_Click(object sender, RoutedEventArgs e)
        {
            sci.TriggerButtonClick();
        }

        #region Tray application
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // deactive the current tab too, as this is a close action of the user and should have the same effect as selecting another tab
                CloseTab();
                this.Hide();

                // exit when we aren't running as tray app
                if (!StartupArguments.RunAsTrayApp)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to handle OK click event", ex);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // When the application is closed, check whether the application is exiting from menu or forms close button
            if (!mIsAppExiting && StartupArguments.RunAsTrayApp)
            {
                // If the form close button is triggered, cancel the event and hide the form.
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                // When we exit the app, make sure to unload the tab and stop USS.
                UserServices.Shutdown();
                CloseTab();
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
                if (!UserServices.USS.IsMediaPortalRunning())
                {
                    UserServices.USS.StartMediaPortal();
                    HandleMediaPortalState(true);
                }
                else
                {
                    UserServices.USS.CloseMediaPortal();
                    HandleMediaPortalState(false);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Exception while trying to start/stop MediaPortal", ex);
            }
        }

        private void HandleMediaPortalState(bool _running)
        {
            if (_running)
            {
                MenuStartCloseMp.Header = UI.TrayCloseMediaPortal;
            }
            else
            {
                MenuStartCloseMp.Header = UI.TrayStartMediaPortal;
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            HandleMediaPortalState(UserServices.USS.IsMediaPortalRunning());
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
            UserServices.USS.SetPowerMode(WebPowerMode.LogOff);
        }

        private void MenuPowermodeSuspend_Click(object sender, RoutedEventArgs e)
        {
            UserServices.USS.SetPowerMode(WebPowerMode.Suspend);
        }

        private void MenuPowermodeHibernate_Click(object sender, RoutedEventArgs e)
        {
            UserServices.USS.SetPowerMode(WebPowerMode.Hibernate);
        }

        private void MenuPowermodeReboot_Click(object sender, RoutedEventArgs e)
        {
            UserServices.USS.SetPowerMode(WebPowerMode.Reboot);
        }

        private void MenuPowermodeShutdown_Click(object sender, RoutedEventArgs e)
        {
            UserServices.USS.SetPowerMode(WebPowerMode.PowerOff);
        }

        private void MenuPowermodeLock_Click(object sender, RoutedEventArgs e)
        {
            UserServices.USS.SetPowerMode(WebPowerMode.Lock);
        }

        private void MenuPowermodeMonitorOff_Click(object sender, RoutedEventArgs e)
        {
            UserServices.USS.SetPowerMode(WebPowerMode.ScreenOff);
        }

        private void MenuPowermodeScreensaverOn_Click(object sender, RoutedEventArgs e)
        {
            UserServices.USS.SetPowerMode(WebPowerMode.Screensaver);
        }

        private void MenuOpenWebMP_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://localhost:" + Configuration.WebMediaPortalHosting.Port));
        }
        #endregion
    }
}
