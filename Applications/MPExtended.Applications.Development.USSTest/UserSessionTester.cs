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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using MPExtended.Services.UserSessionService.Interfaces;

namespace MPExtended.Applications.Development.USSTest
{
    public partial class UserSessionTester : Form
    {
        IUserSessionService service;
        public UserSessionTester()
        {
            InitializeComponent();

            foreach (WebPowerMode p in System.Enum.GetValues(typeof(WebPowerMode)))
            {
                cbPowerModes.Items.Add(p);
            }

            service = ChannelFactory<IUserSessionService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/UserSessionService"));
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            WebResult res = service.StartMediaPortal();
        }

        private void cmdSetPowermode_Click(object sender, EventArgs e)
        {
            if (service != null)
            {
                try
                {
                    WebResult res = service.SetPowerMode((WebPowerMode)cbPowerModes.SelectedItem);
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't connect to service");
                }
            }
        }

        private void cmdStartMp_Click(object sender, EventArgs e)
        {
            if (service != null)
            {
                try
                {
                    WebResult res = service.StartMediaPortal();
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't connect to service");
                }
            }
        }

        private void cmdCloseMp_Click(object sender, EventArgs e)
        {
            if (service != null)
            {
                try
                {
                    WebResult res = service.CloseMediaPortal();
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't connect to service");
                }
            }
        }

        private void cmdMpStatus_Click(object sender, EventArgs e)
        {
            if (service != null)
            {
                try
                {
                    WebResult res = service.IsMediaPortalRunning();
                    if (res.Result)
                    {
                        MessageBox.Show("MediaPortal is running");
                    }
                    else
                    {
                        MessageBox.Show("MediaPortal is not running");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't connect to service");
                }
            }
        }
    }
}
