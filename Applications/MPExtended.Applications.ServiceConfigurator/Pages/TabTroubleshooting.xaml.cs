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
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabTroubleshooting.xaml
    /// </summary>
    public partial class TabTroubleshooting : Page
    {
        private class MyNetworkAddress
        {
            public NetworkInterface Interface { get; set; }
            public IPAddressInformation Address { get; set; }

            public override string ToString()
            {
                return Address.Address + " (" + Interface.Name + ")";
            }
        }

        private Dictionary<string, MyNetworkAddress> networkAddresses;

        public TabTroubleshooting()
        {
            InitializeComponent();
            SetNetworkInterfaces();
        }

        private void SetNetworkInterfaces()
        {
            networkAddresses = new Dictionary<string,MyNetworkAddress>();
            foreach (NetworkInterface n in NetworkInterface.GetAllNetworkInterfaces())
            {
                Log.Debug("Available Network Interface: {0}", n.Name);
                IPInterfaceProperties properties = n.GetIPProperties();

                foreach (IPAddressInformation unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork ||
                        (unicast.Address.AddressFamily == AddressFamily.InterNetworkV6 && Configuration.Services.EnableIPv6))
                    {
                        Log.Debug("\tAvailable UniCast: {0}", unicast.Address);
                        MyNetworkAddress addr = new MyNetworkAddress() { Interface = n, Address = unicast };
                        networkAddresses.Add(addr.ToString(), addr);
                    }
                }
            }

            cbNetworkInterfaces.DataContext = networkAddresses;
            cbNetworkInterfaces.DisplayMemberPath = "Key";
            cbNetworkInterfaces.SelectedValuePath = "Value";

            if(networkAddresses.Count > 0)
            {
                cbNetworkInterfaces.SelectedValue = networkAddresses.First().Value;
            }
        }

        private void cbNetworkInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyNetworkAddress addr = (MyNetworkAddress)cbNetworkInterfaces.SelectedValue;
            if (addr != null)
            {
                SetTestLinks(addr.Address.Address.ToString(), Configuration.Services.Port);
            }
        }

        private void SetTestLinks(string _address, int _port)
        {
            string baseAdress = "http://{0}:{1}/MPExtended/{2}/json/{3}";

            if (Installation.IsServiceInstalled(MPExtendedService.MediaAccessService))
            {
                string mediaAccessServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "MediaAccessService", "GetServiceDescription");
                hlTestLinkMediaAccess.NavigateUri = new Uri(mediaAccessServiceDescriptionUrl);
                tbTestLinkMediaAccess.Text = mediaAccessServiceDescriptionUrl;
            }
            else
            {
                gridLinks.RowDefinitions[0].MaxHeight = 0;
            }

            if (Installation.IsServiceInstalled(MPExtendedService.TVAccessService))
            {
                string tvAccessServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "TVAccessService", "GetServiceDescription");
                hlTestLinkTvAccess.NavigateUri = new Uri(tvAccessServiceDescriptionUrl);
                tbTestLinkTvAccess.Text = tvAccessServiceDescriptionUrl;
            }
            else
            {
                gridLinks.RowDefinitions[1].MaxHeight = 0;
            }

            if (Installation.IsServiceInstalled(MPExtendedService.StreamingService))
            {
                string streamingServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "StreamingService", "GetServiceDescription");
                hlTestLinkStreaming.NavigateUri = new Uri(streamingServiceDescriptionUrl);
                tbTestLinkStreaming.Text = streamingServiceDescriptionUrl;
            }
            else
            {
                gridLinks.RowDefinitions[2].MaxHeight = 0;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".zip";
            dlg.Filter = "Log and configuration archive (.zip)|*.zip";
            if (dlg.ShowDialog() == true)
            {
                LogExporter.Export(dlg.FileName);
                MessageBox.Show(String.Format("Exported logs and configuration data to {0}", dlg.FileName), "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
