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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Navigation;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service.Network;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabConfiguration.xaml
    /// </summary>
    public partial class TabConfiguration : Page, ITabCloseCallback
    {
        private static BackgroundWorker loadLanguagesWorker;
        private static IEnumerable<CultureInfo> availableTranslations;

        public static void StartLoadingTranslations()
        {
            loadLanguagesWorker = new BackgroundWorker();
            loadLanguagesWorker.DoWork += delegate(object source, DoWorkEventArgs args)
            {
                availableTranslations = CultureDatabase.GetAvailableTranslations(UI.ResourceManager).ToList();
            };
            loadLanguagesWorker.RunWorkerAsync();
        }

        public TabConfiguration()
        {
            InitializeComponent();

            // load config
            txtPort.Text = Configuration.Services.Port.ToString();
            txtServiceName.Text = GetServiceName();
            txtNetworkUser.Text = Configuration.Services.NetworkImpersonation.Username;
            txtNetworkPassword.Password = Configuration.Services.NetworkImpersonation.GetPassword();
            cbAccessRequestEnabled.IsChecked = Configuration.Services.AccessRequestEnabled;
            cbAutoDetectExternalIp.IsChecked = Configuration.Services.ExternalAddress.Autodetect;

            if (Configuration.Services.ExternalAddress.Autodetect)
            {
                GetExternalAddress();
            }
            else
            {
                txtCustomExternalAddress.Text = Configuration.Services.ExternalAddress.Custom;
            }

            // load dynamic data
            while (loadLanguagesWorker.IsBusy)
                Thread.Sleep(20);
            LoadLanguageChoices();
            CheckBonjour();
        }

        public void TabClosed()
        {
            Configuration.Services.DefaultLanguage = (string)cbLanguage.SelectedValue;
            Configuration.Services.Port = Int32.Parse(txtPort.Text);
            Configuration.Services.BonjourName = txtServiceName.Text;
            Configuration.Services.BonjourEnabled = cbBonjourEnabled.IsChecked.Value;
            Configuration.Services.NetworkImpersonation.Username = txtNetworkUser.Text;
            Configuration.Services.NetworkImpersonation.SetPasswordFromPlaintext(txtNetworkPassword.Password);
            Configuration.Services.AccessRequestEnabled = cbAccessRequestEnabled.IsChecked.Value;
            Configuration.Services.ExternalAddress.Autodetect = cbAutoDetectExternalIp.IsChecked.Value;

            if (!cbAutoDetectExternalIp.IsChecked.Value)
            {
                Configuration.Services.ExternalAddress.Custom = txtCustomExternalAddress.Text;
            }

            Configuration.Save();
        }

        private void LoadLanguageChoices()
        {
            cbLanguage.DisplayMemberPath = "DisplayName";
            cbLanguage.SelectedValuePath = "Name";
            cbLanguage.DataContext = availableTranslations;

            if (Configuration.Services.DefaultLanguage != null &&
                availableTranslations.Select(x => x.Name).Contains(Configuration.Services.DefaultLanguage))
                cbLanguage.SelectedValue = availableTranslations.First(x => x.Name == Configuration.Services.DefaultLanguage);

            if (cbLanguage.SelectedValue == null &&
                availableTranslations.Contains(Thread.CurrentThread.CurrentUICulture))
                cbLanguage.SelectedValue = Thread.CurrentThread.CurrentUICulture;

            if (cbLanguage.SelectedValue == null)
                cbLanguage.SelectedValue = availableTranslations.First(x => x.Name == "en");
        }

        private void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguage.SelectedItem != null)
            {
                Thread.CurrentThread.CurrentCulture = (CultureInfo)cbLanguage.SelectedItem;
                Thread.CurrentThread.CurrentUICulture = (CultureInfo)cbLanguage.SelectedItem;
            }
        }

        private void CheckBonjour()
        {
            // check if bonjour is enabled
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += delegate(object source, DoWorkEventArgs args)
            {
                args.Result = Zeroconf.CheckBonjourInstallation();
            };
            bw.RunWorkerCompleted += delegate(object source, RunWorkerCompletedEventArgs args)
            {
                tbAutodetection.Inlines.Clear();
                tbAutodetection.Inlines.Add(UI.AutodetectionText);

                if (!(bool)args.Result)
                {
                    tbAutodetection.Inlines.Add(new LineBreak());
                    tbAutodetection.Inlines.Add(UI.BonjourNotInstalled);
                    Hyperlink link = new Hyperlink();
                    link.NavigateUri = new Uri("http://support.apple.com/kb/DL999");
                    link.RequestNavigate += CommonEventHandlers.NavigateHyperlink;
                    link.Inlines.Add(UI.BonjourNotInstalledDownload);
                    tbAutodetection.Inlines.Add(" ");
                    tbAutodetection.Inlines.Add(link);

                    cbBonjourEnabled.IsChecked = false;
                }
                else
                {
                    lblServiceName.IsEnabled = true;
                    cbBonjourEnabled.IsEnabled = true;
                    cbBonjourEnabled.IsChecked = Configuration.Services.BonjourEnabled;
                    txtServiceName.IsEnabled = true;
                }
            };
            bw.RunWorkerAsync();
        }

        private string GetServiceName()
        {
            string value = Configuration.Services.BonjourName;
            if (!String.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            try
            {
                return System.Environment.MachineName;
            }
            catch (Exception)
            {
                return "MPExtended Services";
            }
        }

        private void GetExternalAddress()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += delegate(object source, DoWorkEventArgs args)
            {
                args.Result = ExternalAddress.GetIP().ToString();
            };
            bw.RunWorkerCompleted += delegate(object source, RunWorkerCompletedEventArgs args)
            {
                txtCustomExternalAddress.Text = (string)args.Result;
            };

            bw.RunWorkerAsync();
        }

        private void cbAutoDetectExternalIp_Checked(object sender, RoutedEventArgs e)
        {
            txtCustomExternalAddress.IsEnabled = false;
            GetExternalAddress();
        }

        private void cbAutoDetectExternalIp_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCustomExternalAddress.IsEnabled = true;
        }

        private void btnTestCredentials_Click(object sender, RoutedEventArgs e)
        {
            if (CredentialTester.TestCredentials("", txtNetworkUser.Text, txtNetworkPassword.Password))
            {
                MessageBox.Show(UI.CredentialValidationSuccessful, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(UI.CredentialValidationFailed, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            CommonEventHandlers.NavigateHyperlink(sender, e);
        }
    }
}
