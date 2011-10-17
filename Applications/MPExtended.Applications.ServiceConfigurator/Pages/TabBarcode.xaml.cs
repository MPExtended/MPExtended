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
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
        public TabBarcode()
        {
            InitializeComponent();

            GenerateBarcode(cbIncludeAuth.IsChecked == true);
        }

        private void cbIncludeAuth_Checked(object sender, RoutedEventArgs e)
        {
            GenerateBarcode(cbIncludeAuth.IsChecked == true);
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
            }
        }

        /// <summary>
        /// Generate a QR Barcode with the server information
        /// </summary>
        private void GenerateBarcode(bool _includeAuth)
        {
            try
            {
                ServerDescription desc = new ServerDescription();
                desc.GeneratorApp = "ServiceConfigurator";
                desc.ServiceType = "Client";

                desc.Port = Configuration.Services.Port;
                desc.Name = Configuration.Services.BonjourName;
                desc.HardwareAddresses = GetHardwareAddresses();
                desc.Hostname = GetServiceName();

                IPHostEntry host;
                String localIP = "?";
                StringBuilder localIPs = new StringBuilder();
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Single address field
                        localIP = ip.ToString();

                        // Multiple addresses field
                        if (localIPs.Length > 0)
                        {
                            localIPs.Append(";");
                        }

                        localIPs.Append(ip.ToString());
                    }
                }

                desc.Addresses = (localIPs.Length > 0) ? localIPs.ToString() : "?";

                if (_includeAuth)
                {
                    // FIXME
                    desc.User = Configuration.Services.Users.First().Username;
                    desc.Password = Configuration.Services.Users.First().Password;
                    desc.AuthOptions = 1; //username/password
                }

                Bitmap bm = QRCodeGenerator.Generate(desc.ToJSON());
                imgQRCode.Source = bm.ToWpfBitmap();
            }
            catch (Exception ex)
            {
                Log.Error("Error generating barcode", ex);
            }
        }


        private static String GetHardwareAddresses()
        {
            StringBuilder hardwareAddresses = new StringBuilder();
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up)
                    {
                        String hardwareAddress = adapter.GetPhysicalAddress().ToString();
                        if (!hardwareAddress.Equals(String.Empty) && hardwareAddress.Length == 12)
                        {
                            if (hardwareAddresses.Length > 0)
                            {
                                hardwareAddresses.Append(";");
                            }

                            hardwareAddresses.Append(hardwareAddress);
                        }
                    }
                }
            }
            catch (NetworkInformationException e)
            {
                Log.Warn("Could not get hardware address", e);
            }

            return hardwareAddresses.ToString();
        }

        /// <summary>
        /// Get the machine name or a fallback
        /// </summary>
        /// <returns></returns>
        public static string GetServiceName()
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
