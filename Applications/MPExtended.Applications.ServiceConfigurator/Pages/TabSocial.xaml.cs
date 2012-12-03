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
using MPExtended.Libraries.Service.Strings;
using MPExtended.Libraries.Service.Util;

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
            public IWatchSharingService Implementation { get; set; }
        }

        private class SharingProvider
        {
            public bool Enabled { get; set; }
            public Reference<bool> EnabledSetting { get; set; }
            public Dictionary<string, string> Configuration { get; set; }
            public GroupBox GroupBox { get; set; }
            public CheckBox EnableBox { get; set; }
            public TextBox UsernameBox { get; set; }
            public PasswordBox PasswordBox { get; set; }
            public Label ResultsLabel { get; set; }
            public Button TestButton { get; set; }

            public bool IsDirty { get; set; }
            public bool Valid { get; set; }
            public IWatchSharingService Implementation { get; set; }
            public BackgroundWorker Worker { get; set; }
        }

        private Dictionary<string, SharingProvider> providers;

        public TabSocial()
        {
            InitializeComponent();

            providers = new Dictionary<string, SharingProvider>()
            {
                { "Trakt", new SharingProvider()
                    {
                        Enabled = Configuration.Streaming.WatchSharing.TraktEnabled,
                        EnabledSetting = new Reference<bool>(() => Configuration.Streaming.WatchSharing.TraktEnabled, 
                            x => { Configuration.Streaming.WatchSharing.TraktEnabled = x; }),
                        Configuration = Configuration.Streaming.WatchSharing.TraktConfiguration,
                        GroupBox = gbTrakt,
                        EnableBox = cbTraktEnable,
                        UsernameBox = tbTraktUsername,
                        PasswordBox = tbTraktPassword,
                        ResultsLabel = lblTraktResults,
                        TestButton = btnTraktTest,
                        Implementation = new TraktSharingProvider()
                    }
                },
                { "Follwit", new SharingProvider()
                    {
                        Enabled = Configuration.Streaming.WatchSharing.FollwitEnabled,
                        EnabledSetting = new Reference<bool>(() => Configuration.Streaming.WatchSharing.FollwitEnabled, 
                            x => { Configuration.Streaming.WatchSharing.FollwitEnabled = x; }),
                        Configuration = Configuration.Streaming.WatchSharing.FollwitConfiguration,
                        GroupBox = gbFollwit,
                        EnableBox = cbFollwitEnable,
                        UsernameBox = tbFollwitUsername,
                        PasswordBox = tbFollwitPassword,
                        ResultsLabel = lblFollwitResults,
                        TestButton = btnFollwitTest,
                        Implementation = new FollwitSharingProvider()
                    }
                }
            };

            // load active settings into form
            foreach (var kvp in providers)
            {
                var provider = kvp.Value;
                provider.ResultsLabel.Content = String.Empty;
                if (provider.Enabled)
                {
                    provider.EnableBox.IsChecked = true;
                    provider.UsernameBox.Text = provider.Configuration["username"];
                    provider.PasswordBox.Password = "xxxxxxxx";
                    provider.TestButton.IsEnabled = true;
                    provider.IsDirty = false;
                }
                else
                {
                    provider.EnableBox.IsChecked = false;
                    provider.UsernameBox.IsEnabled = false;
                    provider.PasswordBox.IsEnabled = false;
                    provider.TestButton.IsEnabled = false;
                }
            }
        }

        private void textChanged(object sender, RoutedEventArgs e)
        {
            SharingProvider provider = providers[(string)((sender as Control).Tag)];
            provider.IsDirty = true;
            provider.Valid = false;
        }

        private void checkChange(object sender, RoutedEventArgs e)
        {
            SharingProvider provider = providers[(string)((sender as Control).Tag)];
            provider.Enabled = (sender as CheckBox).IsChecked.GetValueOrDefault(true);
            if (provider.Enabled)
            {
                provider.UsernameBox.IsEnabled = true;
                provider.PasswordBox.IsEnabled = true;
                provider.TestButton.IsEnabled = true;
                provider.IsDirty = true;
                provider.Valid = false;
            }
            else
            {
                provider.UsernameBox.IsEnabled = false;
                provider.PasswordBox.IsEnabled = false;
                provider.TestButton.IsEnabled = false;
                provider.UsernameBox.Text = String.Empty;
                provider.PasswordBox.Password = String.Empty;
                provider.ResultsLabel.Content = String.Empty;
                provider.IsDirty = true;
                provider.Valid = true;
            }
        }


        private void testClick(object sender, RoutedEventArgs e)
        {
            SharingProvider provider = providers[(string)((sender as Control).Tag)];
            provider.ResultsLabel.Foreground = Brushes.Black;
            provider.ResultsLabel.Content = UI.TestingCredentials;
            provider.TestButton.IsEnabled = false;
            provider.UsernameBox.IsEnabled = false;
            provider.PasswordBox.IsEnabled = false;
            provider.Worker = new BackgroundWorker();
            provider.Worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                TestCredentialsData data = (TestCredentialsData)args.Argument;
                args.Result = data.Implementation.TestCredentials(data.Username, data.Password);
            };
            provider.Worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if ((bool)args.Result)
                {
                    provider.Valid = true;
                    provider.ResultsLabel.Content = UI.LoginSuccessful;
                    provider.ResultsLabel.Foreground = Brushes.Green;
                }
                else
                {
                    provider.ResultsLabel.Content = UI.LoginFailed;
                    provider.ResultsLabel.Foreground = Brushes.Red;
                }
                provider.TestButton.IsEnabled = true;
                provider.UsernameBox.IsEnabled = true;
                provider.PasswordBox.IsEnabled = true;
            };
            provider.Worker.RunWorkerAsync(new TestCredentialsData()
            {
                Implementation = provider.Implementation,
                Username = provider.UsernameBox.Text,
                Password = provider.PasswordBox.Password
            });
        }

        public void TabClosed()
        {
            if (!providers.Any(x => x.Value.IsDirty))
                return;

            bool invalid = providers.Any(x => x.Value.IsDirty && !x.Value.Valid);
            if (invalid)
            {
                MessageBox.Show(UI.DiscardingInvalidChangesSocialTab, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var kvp in providers.Where(x => x.Value.IsDirty))
            {
                var provider = kvp.Value;
                if (provider.Enabled)
                {
                    provider.EnabledSetting.Value = true;
                    provider.Configuration["username"] = provider.UsernameBox.Text;
                    provider.Configuration["passwordHash"] = provider.Implementation.HashPassword(provider.PasswordBox.Password);
                }
                else
                {
                    provider.EnabledSetting.Value = false;
                    provider.Configuration["username"] = String.Empty;
                    provider.Configuration["passwordHash"] = String.Empty;
                }
            }
            Configuration.Save();
        }
    }
}
