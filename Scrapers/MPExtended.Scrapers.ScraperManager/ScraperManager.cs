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
using BrightIdeasSoftware;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Scrapers.ScraperManager
{
    public partial class ScraperManager : Form
    {
        private IScraperService proxyChannel;

        private WebScraper selected;
        private WebScraperStatus currentStatus;
        private List<WebScraperInputRequest> requests;
        private SearchResultForm dialog;
        private List<WebScraperItem> _listItems;

        private IScraperService Proxy
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
                    BasicHttpBinding binding = new BasicHttpBinding()
                    {
                        MaxReceivedMessageSize = Int32.MaxValue,
                        ReceiveTimeout = new TimeSpan(1, 0, 0),
                        SendTimeout = new TimeSpan(1, 0, 0),
                    };

                    binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

                    var endpointAddress = new EndpointAddress("http://10.1.0.166:4322/MPExtended/ScraperService");
                    var factory = new ChannelFactory<IScraperService>(binding, endpointAddress);
                    factory.Credentials.UserName.UserName = "diebagger";
                    factory.Credentials.UserName.Password = "Opperate20";


                    proxyChannel = factory.CreateChannel();

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
            chItemTitle.AspectGetter = delegate(object row) { return ((WebScraperItem)row).Title; };
            chItemState.AspectGetter = delegate(object row) { return ((WebScraperItem)row).State; };

            chItemProgress.AspectGetter = delegate(object row)
            {
                WebScraperItem item = (WebScraperItem)row;
                if (item.Progress >= 0)
                {
                    return item.Progress;
                }
                else
                {
                    return "";
                }
            };

            _listItems = new List<WebScraperItem>();
            olvScraperItems.SetObjects(_listItems);

            IList<WebScraper> scrapers = Proxy.GetAvailableScrapers();

            foreach (WebScraper s in scrapers)
            {
                cbAvailableScrapers.Items.Add(s);
            }

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

            selected = (WebScraper)cbAvailableScrapers.SelectedItem;

            timerUpdateScraperState.Start();
        }

        private void cmdStart_Click(object sender, EventArgs e)
        {
            bool result = Proxy.StartScraper(selected.ScraperId);
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            Proxy.TriggerUpdate(selected.ScraperId);
        }

        private void cmdPauseResume_Click(object sender, EventArgs e)
        {
            if (currentStatus.ScraperState == WebScraperState.Paused)
            {
                bool result = Proxy.ResumeScraper(selected.ScraperId);
            }
            else
            {
                bool result = Proxy.PauseScraper(selected.ScraperId);
            }
        }

        private void cmdStop_Click(object sender, EventArgs e)
        {
            bool result = Proxy.StopScraper(selected.ScraperId);
        }

        private void timerUpdateScraperState_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Proxy != null)
                {
                    currentStatus = Proxy.GetScraperStatus(selected.ScraperId);
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
                        reqs = Proxy.GetAllScraperInputRequests(selected.ScraperId);
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

                    List<WebScraperItem> items = Proxy.GetScraperItems(selected.ScraperId);
                        UpdateItems(items);                  
                    
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

        private void UpdateItems(List<WebScraperItem> items)
        {
            for(int i = 0; i < _listItems.Count; i++)
            {
                bool found = UpdateItem(_listItems[i], items);

                if (!found)
                {
                    _listItems.RemoveAt(i);
                    i--;
                }
            }

            if (items.Count > 0)
            {
                _listItems.AddRange(items);
                olvScraperItems.SetObjects(_listItems);
            }
        }

        private bool UpdateItem(WebScraperItem oldItem, List<WebScraperItem> newItems)
        {
            foreach (WebScraperItem i in newItems)
            {
                if (i.ItemId == oldItem.ItemId)
                {
                    if (i.LastUpdated.Ticks > oldItem.LastUpdated.Ticks)
                    {
                        oldItem.LastUpdated = i.LastUpdated;
                        oldItem.ItemActions = i.ItemActions;
                        oldItem.Progress = i.Progress;
                        oldItem.State = i.State;
                        oldItem.Title = i.Title;
                            
                        olvScraperItems.RefreshObject(oldItem);
                        olvScraperItems.Refresh();
                    }
                    newItems.Remove(i);
                    return true;
                }
            }
            return false;
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

                dialog = new SearchResultForm(Proxy, selected, request);
                DialogResult res = dialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    if (dialog.NewSearchText != null)
                    {
                        Proxy.SetScraperInputRequest(selected.ScraperId, request.Id, null, dialog.NewSearchText);
                    }
                    else
                    {
                        WebScraperInputMatch match = dialog.SelectedMatch;
                        Proxy.SetScraperInputRequest(selected.ScraperId, request.Id, match.Id, null);
                    }
                }
            }
        }

        private void cmdAddDownload_Click(object sender, EventArgs e)
        {
            String title = txtItemName.Text != "" ? txtItemName.Text : txtItemId.Text;
            String type = (String)cbItemType.SelectedItem;
            if (type == "TV Episode")
            {
                Proxy.AddItemToScraper(selected.ScraperId, title, WebMediaType.TVEpisode, 6, txtItemId.Text, 0);
            }
            else if (type == "Movie")
            {
                Proxy.AddItemToScraper(selected.ScraperId, title, WebMediaType.Movie, 3, txtItemId.Text, 0);
            }
        }

        private void cmsItemActions_Opening(object sender, CancelEventArgs e)
        {
            if (olvScraperItems.SelectedIndex != -1)
            {
                WebScraperItem selected = _listItems[olvScraperItems.SelectedIndex];

                cmsItemActions.Items.Clear();

                foreach (WebScraperAction a in selected.ItemActions)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem();
                    item.Text = a.Title;
                    item.ToolTipText = a.Description;
                    item.Enabled = a.Enabled;
                    item.Tag = a;
                    cmsItemActions.Items.Add(item);
                }
            }
        }

        private void cmsItemActions_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            WebScraperItem item = _listItems[olvScraperItems.SelectedIndex];

            WebScraperAction action = e.ClickedItem.Tag as WebScraperAction;

            Proxy.InvokeScraperAction(selected.ScraperId, item.ItemId, action.ActionId);
        }
    }
}
