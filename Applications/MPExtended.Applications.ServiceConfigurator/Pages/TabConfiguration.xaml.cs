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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Navigation;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service.Network;
using WpfMessageBox = System.Windows.MessageBox;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabConfiguration.xaml
    /// </summary>
    public partial class TabConfiguration : Page, ITabCloseCallback
    {
        private static Task<List<CultureInfo>> languageLoadingTask;

        public static void StartLoadingTranslations()
        {
            languageLoadingTask = Task<List<CultureInfo>>.Factory.StartNew(() => CultureDatabase.GetAvailableTranslations(UI.ResourceManager).ToList());
        }

        public TabConfiguration()
        {
            InitializeComponent();

            // load config
            txtPort.Text = Configuration.Services.Port.ToString();
            txtServiceName.Text = GetServiceName();
            txtTVLogoDirectory.Text = Configuration.Streaming.GetAbsoluteTVLogoDirectory();
            txtNetworkUser.Text = String.IsNullOrEmpty(Configuration.Services.NetworkImpersonation.Domain) ?
                Configuration.Services.NetworkImpersonation.Username :
                Configuration.Services.NetworkImpersonation.Domain + "\\" + Configuration.Services.NetworkImpersonation.Username;
            txtNetworkPassword.Password = Configuration.Services.NetworkImpersonation.GetPassword();
            cbAccessRequestEnabled.IsChecked = Configuration.Services.AccessRequestEnabled;

            // if autodetection is enabled, setting this property fires the _Checked event which loads the address
            cbAutoDetectExternalIp.IsChecked = Configuration.Services.ExternalAddress.Autodetect;
            if (!Configuration.Services.ExternalAddress.Autodetect)
                txtCustomExternalAddress.Text = Configuration.Services.ExternalAddress.Custom;

            // load dynamic data
            LoadLanguageChoices();
            CheckBonjour();
        }

        public void TabClosed()
        {
            Configuration.Services.DefaultLanguage = (string)cbLanguage.SelectedValue;
            Configuration.Services.Port = Int32.Parse(txtPort.Text);
            Configuration.Services.BonjourName = txtServiceName.Text;
            Configuration.Services.BonjourEnabled = cbBonjourEnabled.IsChecked.Value;

            var domuser = GetDomainAndUsername(txtNetworkUser.Text);
            Configuration.Services.NetworkImpersonation.Domain = domuser.Item1;
            Configuration.Services.NetworkImpersonation.Username = domuser.Item2;
            Configuration.Services.NetworkImpersonation.SetPasswordFromPlaintext(txtNetworkPassword.Password);

            Configuration.Streaming.TVLogoDirectory = txtTVLogoDirectory.Text;
            Configuration.Services.AccessRequestEnabled = cbAccessRequestEnabled.IsChecked.Value;

            Configuration.Services.ExternalAddress.Autodetect = cbAutoDetectExternalIp.IsChecked.Value;
            if (!cbAutoDetectExternalIp.IsChecked.Value)
                Configuration.Services.ExternalAddress.Custom = txtCustomExternalAddress.Text;

            Configuration.Save();
        }

        private void LoadLanguageChoices()
        {
            var languages = languageLoadingTask.Result;
            cbLanguage.DisplayMemberPath = "DisplayName";
            cbLanguage.SelectedValuePath = "Name";
            cbLanguage.DataContext = languages;

            if (Configuration.Services.DefaultLanguage != null &&
                languages.Select(x => x.Name).Contains(Configuration.Services.DefaultLanguage))
                cbLanguage.SelectedValue = languages.First(x => x.Name == Configuration.Services.DefaultLanguage);

            if (cbLanguage.SelectedValue == null &&
                languages.Contains(Thread.CurrentThread.CurrentUICulture))
                cbLanguage.SelectedValue = Thread.CurrentThread.CurrentUICulture;

            if (cbLanguage.SelectedValue == null)
                cbLanguage.SelectedValue = languages.First(x => x.Name == "en");
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
                    tbAutodetection.Inlines.Add(UI.BonjourNotInstalled.Trim());
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
                var ip = ExternalAddress.GetIP();
                args.Result = ip != null ? ip.ToString() : String.Empty;
            };
            bw.RunWorkerCompleted += delegate(object source, RunWorkerCompletedEventArgs args)
            {
                txtCustomExternalAddress.Text = (string)args.Result;
            };

            bw.RunWorkerAsync();
        }

        private Tuple<string, string> GetDomainAndUsername(string input)
        {
            return input.Contains('\\') ?
                Tuple.Create(input.Substring(0, input.IndexOf('\\')), input.Substring(input.IndexOf('\\') + 1)) :
                Tuple.Create(String.Empty, input);
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
            var domuser = GetDomainAndUsername(txtNetworkUser.Text);
            if (CredentialTester.TestCredentials(domuser.Item1, domuser.Item2, txtNetworkPassword.Password))
            {
                WpfMessageBox.Show(UI.CredentialValidationSuccessful, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                WpfMessageBox.Show(UI.CredentialValidationFailed, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            CommonEventHandlers.NavigateHyperlink(sender, e);
        }

        private void btnBrowseTVLogoDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            dialog.SelectedPath = txtTVLogoDirectory.Text;
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                txtTVLogoDirectory.Text = dialog.SelectedPath;
        }
    }
}
