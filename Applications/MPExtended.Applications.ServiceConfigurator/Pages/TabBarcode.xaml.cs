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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabBarcode.xaml
    /// </summary>
    public partial class TabBarcode : Page
    {
        private Dictionary<string, User> Users;

        public TabBarcode()
        {
            InitializeComponent();

            Users = Configuration.Services.Users.ToDictionary(x => x.Username, x => x);
            cbUser.DataContext = Users;
            cbUser.DisplayMemberPath = "Key";
            cbUser.SelectedValuePath = "Value";
            cbUser.SelectedIndex = 0;

            GenerateBarcode(null);
        }

        private void updateBarcode(object sender, EventArgs e)
        {
            if (cbIncludeAuth.IsChecked == true)
            {
                GenerateBarcode((User)cbUser.SelectedValue);
            } 
            else 
            {
                GenerateBarcode(null);
            }
        }

        /// <summary>
        /// Save the generated barcode as image file to harddisk
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.Filter = "JPEG Image|*.jpg";
            diag.Title = "Save Barcode as Image File";
            if (diag.ShowDialog() == true)
            {
                ((BitmapSource)imgQRCode.Source).ToWinFormsBitmap().Save(diag.FileName);
                MessageBox.Show(String.Format("Saved QR-code to {0}", diag.FileName), "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Generate a QR Barcode with the server information
        /// </summary>
        private void GenerateBarcode(User auth)
        {
            try
            {
                ServerDescription desc = new ServerDescription();
                desc.HardwareAddresses = String.Join(";", NetworkInformation.GetMACAddresses());
                desc.Addresses = String.Join(";", NetworkInformation.GetIPAddresses());
                desc.Name = GetServiceName();
                desc.QRVersion = 1;

                desc.Services = new List<ServiceDescription>();
                foreach (var srv in Installation.GetInstalledServices())
                {
                    var srvdesc = new ServiceDescription()
                    {
                        Name = srv.ServiceName.ToString(),
                        Port = srv.Port
                    };

                    if (auth != null)
                    {
                        string usernameOut, passwordOut;
                        srv.GetUsernameAndPassword(auth, out usernameOut, out passwordOut);
                        srvdesc.User = usernameOut;
                        srvdesc.Password = passwordOut;
                    }

                    desc.Services.Add(srvdesc);
                }

                Bitmap bm = QRCodeGenerator.Generate(desc.ToJSON());
                imgQRCode.Source = bm.ToWpfBitmap();
            }
            catch (Exception ex)
            {
                Log.Error("Error generating barcode", ex);
            }
        }

        /// <summary>
        /// Get the machine name or a fallback
        /// </summary>
        private static string GetServiceName()
        {
            try
            {
                return System.Environment.MachineName;
            }
            catch (InvalidOperationException)
            {
                return "MPExtended Service";
            }
        }
    }
}
