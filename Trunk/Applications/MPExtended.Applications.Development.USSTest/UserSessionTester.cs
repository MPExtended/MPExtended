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
                catch (Exception ex)
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
                catch (Exception ex)
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
                catch (Exception ex)
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
                    if (res.Status)
                    {
                        MessageBox.Show("MediaPortal is running");
                    }
                    else
                    {
                        MessageBox.Show("MediaPortal is not running");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Couldn't connect to service");
                }
            }
        }
    }
}
