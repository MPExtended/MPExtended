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
using System.ComponentModel;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Social;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabSocial.xaml
    /// </summary>
    public partial class TabSocial : Page, ITabCloseCallback
    {
        private class TestCredentialsData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private string activeProvider;
        private BackgroundWorker worker;

        private bool dirty;
        private bool canSave;

        public TabSocial()
        {
            InitializeComponent();
            lblTestResult.Content = "";
            canSave = true;

            if (Configuration.Streaming.WatchSharing["type"] == "none" || Configuration.Streaming.WatchSharing["type"] == "debug") // debug isn't supported in UI
            {
                rbNone.IsChecked = true;
            }
            else
            {
                if (Configuration.Streaming.WatchSharing["type"] == "follwit")
                    rbFollwit.IsChecked = true;

                if (Configuration.Streaming.WatchSharing["type"] == "trakt")
                    rbTrakt.IsChecked = true;

                txtUsername.Text = Configuration.Streaming.WatchSharing["username"];
            }

            // make sure to set this after the setting of the radio boxes, to avoid saving when nothing has been changed
            dirty = false;
        }

        private void radioButtonChanged(object sender, RoutedEventArgs e)
        {
            activeProvider = (string)((e.Source as RadioButton).Tag);
            InvalidateTestResults();
            switch (activeProvider)
            {
                case "none":
                    btnTest.IsEnabled = false;
                    txtUsername.IsEnabled = false;
                    txtPassword.IsEnabled = false;
                    break;
                case "follwit":
                case "trakt":
                    btnTest.IsEnabled = true;
                    txtUsername.IsEnabled = true;
                    txtPassword.IsEnabled = true;
                    break;
            }
        }

        private void textChanged(object sender, RoutedEventArgs e)
        {
            InvalidateTestResults();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            lblTestResult.Foreground = Brushes.Black;
            lblTestResult.Content = Strings.UI.TestingCredentials;
            btnTest.IsEnabled = false;
            worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                TestCredentialsData data = args.Argument as TestCredentialsData;
                IWatchSharingService service = GetImplementation();
                args.Result = service.TestCredentials(data.Username, data.Password);
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if ((bool)args.Result)
                {
                    lblTestResult.Content = Strings.UI.LoginSuccessful;
                    lblTestResult.Foreground = Brushes.Green;
                    canSave = true;
                }
                else
                {
                    lblTestResult.Content = Strings.UI.LoginFailed;
                    lblTestResult.Foreground = Brushes.Red;
                    canSave = false;
                }
                btnTest.IsEnabled = true;
            };
            worker.RunWorkerAsync(new TestCredentialsData()
            {
                Username = txtUsername.Text,
                Password = txtPassword.Password
            });
        }

        private IWatchSharingService GetImplementation()
        {
            if (activeProvider == "follwit")
            {
                return new FollwitSharingProvider();
            }
            else if (activeProvider == "trakt")
            {
                return new TraktSharingProvider();
            }
            return null;
        }

        private void InvalidateTestResults()
        {
            lblTestResult.Content = "";
            dirty = true;
            canSave = false;
        }

        public void TabClosed()
        {
            if (canSave && dirty)
            {
                Configuration.Streaming.WatchSharing["username"] = txtUsername.Text;
                Configuration.Streaming.WatchSharing["passwordHash"] = GetImplementation().HashPassword(txtPassword.Password);
                Configuration.Streaming.WatchSharing["type"] = activeProvider;
                Configuration.Streaming.Save();
            }
            else if (dirty)
            {
                MessageBox.Show(Strings.UI.DiscardingInvalidChangesSocialTab, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
