﻿#region Copyright (C) 2011-2013 MPExtended
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Network;
using MPExtended.Libraries.Service.Strings;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabTroubleshooting.xaml
    /// </summary>
    public partial class TabTroubleshooting : Page
    {
        private struct NetworkInterface
        {
            public string InterfaceName { get; set; }
            public IPAddress Address { get; set; }

            public override string ToString()
            {
                return String.Format("{0} ({1})", InterfaceName, Address);
            }
        }

        private IEnumerable<NetworkInterface> networkAddresses;

        public TabTroubleshooting()
        {
            InitializeComponent();
            SetNetworkInterfaces();
        }

        private void SetNetworkInterfaces()
        {
            networkAddresses = NetworkInformation.GetNetworkInterfaces()
                .SelectMany(x => x.Addresses.Select(y => new NetworkInterface() { InterfaceName = x.Name, Address = y }));
            cbNetworkInterfaces.DataContext = networkAddresses;

            if (networkAddresses.Count() > 0)
                cbNetworkInterfaces.SelectedIndex = 0;
        }

        private void cbNetworkInterfaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbNetworkInterfaces.SelectedItem != null)
                SetTestLinks(((NetworkInterface)cbNetworkInterfaces.SelectedItem).Address, Configuration.Services.Port);
        }

        private void SetTestLinks(IPAddress _address, int _port)
        {
            string baseAdress = _address.AddressFamily == AddressFamily.InterNetworkV6 ? 
                "http://[{0}]:{1}/MPExtended/{2}/json/{3}" : 
                "http://{0}:{1}/MPExtended/{2}/json/{3}";

            var items = new List<Tuple<string, int, Hyperlink, TextBlock>>
            {
                Tuple.Create("MediaAccessService", 0, hlTestLinkMediaAccess, tbTestLinkMediaAccess),
                Tuple.Create("TVAccessService", 1, hlTestLinkTvAccess, tbTestLinkTvAccess),
                Tuple.Create("StreamingService", 2, hlTestLinkStreaming, tbTestLinkStreaming)
            };

            foreach (var service in items)
            {
                if (Installation.IsServiceInstalled(service.Item1))
                {
                    var url = String.Format(baseAdress, _address, _port, service.Item1, "GetServiceDescription");
                    service.Item3.NavigateUri = new Uri(url);
                    service.Item4.Text = url;
                }
                else
                {
                    gridLinks.RowDefinitions[service.Item2].MaxHeight = 0;
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            LogExporter.ExportWithFileChooser();
        }

        private void btnCleanCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // do not call Installation.GetCacheDirectory() twice as it also creates the directory
                string directory = Installation.GetCacheDirectory();
                Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);

                MessageBox.Show(UI.CleanCacheSucceeded, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to clean cache", ex);
                ErrorHandling.OnlyShowError(ex);
            }
        }
    }
}
