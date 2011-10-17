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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabTroubleshooting.xaml
    /// </summary>
    public partial class TabTroubleshooting : Page
    {
        public TabTroubleshooting()
        {
            InitializeComponent();
        }

        internal class MyNetworkAddress
        {
            internal NetworkInterface Interface { get; set; }
            internal IPAddressInformation Address { get; set; }

            public override string ToString()
            {
                return Address.Address + " (" + Interface.Name + ")";
            }
        }

        private void SetNetworkInterfaces()
        {
            cbNetworkInterfaces.Items.Clear();

            foreach (NetworkInterface n in NetworkInterface.GetAllNetworkInterfaces())
            {
                Log.Debug("Available Network Interface: {0}", n.Name);
                IPInterfaceProperties properties = n.GetIPProperties();

                foreach (IPAddressInformation unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Log.Debug("\tAvailable UniCast: {0}", unicast.Address);
                        cbNetworkInterfaces.Items.Add(new MyNetworkAddress() { Interface = n, Address = unicast });
                    }
                }
            }

            SetTestLinks("localhost", 4322);

        }

        private void cbNetworkInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyNetworkAddress addr = (MyNetworkAddress)cbNetworkInterfaces.SelectedItem;
            if (addr != null)
            {
                SetTestLinks(addr.Address.Address.ToString(), 4322);
            }
        }

        private void SetTestLinks(string _address, int _port)
        {
            String baseAdress = "http://{0}:{1}/MPExtended/{2}/json/{3}";

            String mediaAccessServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "MediaAccessService", "GetServiceDescription");
            hlTestLinkMediaAccessGeneral.NavigateUri = new Uri(mediaAccessServiceDescriptionUrl);
            tbTestLinkMediaAccessGeneral.Text = mediaAccessServiceDescriptionUrl;

            String tvAccessServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "TVAccessService", "GetServiceDescription");
            hlTestLinkTvAccessGeneral.NavigateUri = new Uri(tvAccessServiceDescriptionUrl);
            tbTestLinkTvAccessGeneral.Text = tvAccessServiceDescriptionUrl;

            String streamingServiceDescriptionUrl = String.Format(baseAdress, _address, _port, "StreamingService", "GetServiceDescription");
            hlTestLinkStreamingGeneral.NavigateUri = new Uri(streamingServiceDescriptionUrl);
            tbTestLinkStreamingGeneral.Text = streamingServiceDescriptionUrl;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
