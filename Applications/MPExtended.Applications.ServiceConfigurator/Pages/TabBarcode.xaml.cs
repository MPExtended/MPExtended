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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service.Network;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabBarcode.xaml
    /// </summary>
    public partial class TabBarcode : Page
    {
        private const int QR_VERSION = 1;
        private const string GENERATOR_APP = "MPExtended";

        private Dictionary<string, User> Users;

        public TabBarcode()
        {
            InitializeComponent();

            // The GroupBy() here is needed because apparantly people manage to create duplicate accounts
            Users = Configuration.Authentication.Users.GroupBy(x => x.Username).ToDictionary(x => x.Key, x => x.First());
            cbUser.DataContext = Users;
            cbUser.DisplayMemberPath = "Key";
            cbUser.SelectedValuePath = "Value";
            cbUser.SelectedIndex = 0;
        }

        private void UpdateBarcode(object sender, EventArgs e)
        {
            UpdateBarcode();
        }

        private void UpdateBarcode()
        {
            var user = cbIncludeAuth.IsChecked == true ? (User)cbUser.SelectedValue : null;
            Task.Factory.StartNew(delegate()
            {
                var bitmap = GenerateBarcode(user);
                Dispatcher.Invoke(new Action(() => imgQRCode.Source = (BitmapSource)bitmap));
            });
        }

        /// <summary>
        /// Save the generated barcode as image file to harddisk
        /// </summary>
        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.Filter = "JPEG Image|*.jpg";
            diag.Title = UI.SaveBarcodeAsImageFile;
            if (diag.ShowDialog() == true)
            {
                ((BitmapSource)imgQRCode.Source).ToWinFormsBitmap().Save(diag.FileName);
                MessageBox.Show(String.Format(UI.SavedBarcodeTo, diag.FileName), "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Generate a QR Barcode with the server information
        /// </summary>
        private BitmapSource GenerateBarcode(User auth)
        {
            try
            {
                ServerDescription desc = new ServerDescription();

                desc.QRVersion = QR_VERSION;
                desc.GeneratorApp = GENERATOR_APP;

                desc.HardwareAddresses = String.Join(";", NetworkInformation.GetMACAddresses());
                desc.Addresses = String.Join(";", NetworkInformation.GetIPAddresses());
                desc.Name = Configuration.Services.GetServiceName();
                desc.NetbiosName = System.Environment.MachineName;
                desc.ExternalIp = ExternalAddress.GetAddress();

                desc.Services = new List<ServiceDescription>();
                User wifiRemoteAuth = WifiRemote.IsInstalled ? WifiRemote.GetAuthentication() : null;
                foreach (var srv in Installation.GetInstalledServices())
                {
                    var srvdesc = new ServiceDescription()
                    {
                        Name = srv.Service.ToString(),
                        Port = srv.Port
                    };

                    if (auth != null)
                    {
                        srvdesc.User = (srv.Service == MPExtendedService.WifiRemote ? wifiRemoteAuth : auth).Username;
                        srvdesc.Password = (srv.Service == MPExtendedService.WifiRemote ? wifiRemoteAuth : auth).GetPassword();
                    }

                    desc.Services.Add(srvdesc);
                }

                Bitmap bm = QRCodeGenerator.Generate(desc.ToJSON());
                return bm.ToWpfBitmap();
            }
            catch (Exception ex)
            {
                Log.Error("Error generating barcode", ex);
                return null;
            }
        }
    }
}
