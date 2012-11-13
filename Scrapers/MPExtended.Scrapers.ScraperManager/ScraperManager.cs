using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MPExtended.Services.ScraperService.Interfaces;
using System.ServiceModel;

namespace MPExtended.Scrapers.ScraperManager
{
    public partial class ScraperManager : Form
    {
        public class ProxyInfo
        {
            public String Name { get; set; }
            public String Address { get; set; }

            public override String ToString()
            {
                return Name;
            }
        }

        private IPrivateScraperService proxyChannel;
        private ProxyInfo selected;
        private WebScraperStatus currentStatus;
        private List<WebScraperInputRequest> requests;
        private SearchResultForm dialog;

        private IPrivateScraperService Proxy
        {
            get
            {
                bool recreateChannel = false;
                if (proxyChannel == null)
                {
                    recreateChannel = true;
                }
                else if (((ICommunicationObject)proxyChannel).State == CommunicationState.Faulted)
                {
                    try
                    {
                        ((ICommunicationObject)proxyChannel).Close(TimeSpan.FromMilliseconds(500));
                    }
                    catch (Exception)
                    {
                        // oops. 
                    }

                    recreateChannel = true;
                }

                if (recreateChannel)
                {
                    NetTcpBinding binding = new NetTcpBinding()
                    {
                        MaxReceivedMessageSize = 100000000,
                        ReceiveTimeout = new TimeSpan(0, 0, 10),
                        SendTimeout = new TimeSpan(0, 0, 10),
                    };
                    binding.ReliableSession.Enabled = true;
                    binding.ReliableSession.Ordered = true;

                    proxyChannel = ChannelFactory<IPrivateScraperService>.CreateChannel(
                        binding,
                        new EndpointAddress(selected.Address)
                    );
                }

                return proxyChannel;
            }
        }

        public ScraperManager()
        {
            InitializeComponent();
            requests = new List<WebScraperInputRequest>();
        }

        private void ScraperManager_Load(object sender, EventArgs e)
        {
            cbAvailableScrapers.Items.Add(new ProxyInfo() { Name = "TvSeries", Address = "net.tcp://localhost:9760/MPExtended/ScraperServiceTVSeries" });
            cbAvailableScrapers.Items.Add(new ProxyInfo() { Name = "Movies", Address = "net.tcp://localhost:9761/MPExtended/ScraperServiceMopi" });
            
            cbAvailableScrapers.SelectedIndex = 0;
            timerUpdateScraperState.Start();
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {

        }

        private void cbAvailableScrapers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (proxyChannel != null)
            {
                try
                {
                    ((ICommunicationObject)proxyChannel).Close(TimeSpan.FromMilliseconds(500));
                }
                catch (Exception)
                {
                    // oops. 
                }

                proxyChannel = null;
            }

            cmdStart.Enabled = true;
            cmdPauseResume.Enabled = false;
            cmdStop.Enabled = false;
            cmdRefresh.Enabled = false;

            selected = (ProxyInfo)cbAvailableScrapers.SelectedItem;

            timerUpdateScraperState.Start();
        }

        private void cmdStart_Click(object sender, EventArgs e)
        {
            bool result = Proxy.StartScraper();
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            Proxy.TriggerUpdate();
        }

        private void cmdPauseResume_Click(object sender, EventArgs e)
        {
            if (currentStatus.ScraperState == WebScraperState.Paused)
            {
                bool result = Proxy.ResumeScraper();
            }
            else
            {
                bool result = Proxy.PauseScraper();
            }
        }

        private void cmdStop_Click(object sender, EventArgs e)
        {
            bool result = Proxy.StopScraper();
        }

        private void timerUpdateScraperState_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Proxy != null)
                {
                    currentStatus = Proxy.GetScraperStatus();
                    tsCurrentStatus.Text = currentStatus.CurrentAction;
                    lblScraperState.Text = currentStatus.ScraperState.ToString();

                    if (currentStatus.ScraperState == WebScraperState.Paused)
                    {
                        cmdPauseResume.Text = "Resume";
                    }
                    else
                    {
                        cmdPauseResume.Text = "Pause";
                    }

                    cmdStart.Enabled = currentStatus.ScraperState == WebScraperState.Stopped;
                    cmdPauseResume.Enabled = currentStatus.ScraperState != WebScraperState.Stopped;
                    cmdStop.Enabled = currentStatus.ScraperState != WebScraperState.Stopped;
                    cmdRefresh.Enabled = currentStatus.ScraperState != WebScraperState.Stopped;

                    IList<WebScraperInputRequest> reqs = null;
                    if (currentStatus.InputNeeded > 0)
                    {
                        reqs = Proxy.GetAllScraperInputRequests();
                    }
                    List<WebScraperInputRequest> newReqs = GetNewReqs(reqs);
                    List<WebScraperInputRequest> oldReqs = GetOldReqs(reqs);

                    foreach (WebScraperInputRequest r in oldReqs)
                    {
                        requests.Remove(r);
                        cbInputRequests.Items.Remove(r);
                    }

                    foreach (WebScraperInputRequest r in newReqs)
                    {
                        requests.Add(r);
                        cbInputRequests.Items.Add(r);
                    }
                }

            }
            catch (EndpointNotFoundException ex)
            {
                timerUpdateScraperState.Stop();
            }
            catch (Exception ex)
            {

            }
        }

        private List<WebScraperInputRequest> GetOldReqs(IList<WebScraperInputRequest> oldReqs)
        {
            List<WebScraperInputRequest> retList = new List<WebScraperInputRequest>();
            foreach (WebScraperInputRequest r in requests)
            {
                bool found = false;
                if (oldReqs != null)
                {
                    foreach (WebScraperInputRequest o in oldReqs)
                    {
                        if (r.Id.Equals(o.Id))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    retList.Add(r);
                }
            }
            return retList;
        }

        private List<WebScraperInputRequest> GetNewReqs(IList<WebScraperInputRequest> newReqs)
        {
            List<WebScraperInputRequest> retList = new List<WebScraperInputRequest>();
            if (newReqs != null)
            {
                foreach (WebScraperInputRequest n in newReqs)
                {
                    bool found = false;
                    foreach (WebScraperInputRequest r in requests)
                    {
                        if (r.Id.Equals(n.Id))
                        {
                            r.InputOptions = n.InputOptions;
                            r.InputType = n.InputType;
                            if (dialog != null)
                            {
                                dialog.UpdateRequest(r);
                            }
                            found = true;
                            break;
                        }
                    }
                    if (!found && n.Id != null)
                    {
                        retList.Add(n);
                    }
                }
            }
            return retList;
        }

        private void cmdMatchItems_Click(object sender, EventArgs e)
        {
            if (cbInputRequests.SelectedItem != null)
            {
                WebScraperInputRequest request = (WebScraperInputRequest)cbInputRequests.SelectedItem;

                dialog = new SearchResultForm(Proxy, request);
                DialogResult res = dialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    if (dialog.NewSearchText != null)
                    {
                        Proxy.SetScraperInputRequest(request.Id, null, dialog.NewSearchText);
                    }
                    else
                    {
                        WebScraperInputMatch match = dialog.SelectedMatch;
                        Proxy.SetScraperInputRequest(request.Id, match.Id, null);
                    }
                }
            }
        }
    }
}
