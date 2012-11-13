using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Scrapers.ScraperManager
{
    public partial class SearchResultForm : Form
    {
        IPrivateScraperService scraper;
        private DialogResult result = DialogResult.Cancel;
        private WebScraperInputRequest request = null;
        private WebScraperInputMatch selection = null;
        private bool mNeedsRefresh;
        public String NewSearchText { get; set; }

        public WebScraperInputMatch SelectedMatch { get { return selection; } }
        public SearchResultForm()
        {
            InitializeComponent();
        }

        public SearchResultForm(WebScraperInputRequest _req)
            : this()
        {
            request = _req;
            FillSearchResults();
        }

        public SearchResultForm(IPrivateScraperService _scraper)
            : this()
        {
            scraper = _scraper;
            FillSearchResults();
        }

        public SearchResultForm(IPrivateScraperService _scraper, WebScraperInputRequest _req)
            : this()
        {
            scraper = _scraper;
            request = _req;
            FillSearchResults();
        }

        private void FillSearchResults()
        {
            lvSearchResult.Items.Clear();
            if (request != null && request.InputOptions != null && request.InputOptions.Count > 0)
            {
                for (int i = 0; i < request.InputOptions.Count; i++)
                {
                    ListViewItem item = new ListViewItem(request.InputOptions[i].ImdbId);
                    item.SubItems.Add(request.InputOptions[i].Title);
                    item.Tag = request.InputOptions[i];
                    lvSearchResult.Items.Add(item);
                }
            }
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            /*List<TvdbSearchResult> res = m_tvdbHandler.SearchSeries(txtSeriesName.Text);
            lvSearchResult.Items.Clear();
            foreach (TvdbSearchResult r in res)
            {
                ListViewItem item = new ListViewItem(r.Id.ToString());
                item.SubItems.Add(r.SeriesName);
                item.SubItems.Add(r.Language.Name);
                item.Tag = r;
                lvSearchResult.Items.Add(item);
            }*/
            scraper.SetScraperInputRequest(request.Id, null, txtSeriesName.Text);
            mNeedsRefresh = true;
            lvSearchResult.Items.Clear();
        }

        private void cmdChoose_Click(object sender, EventArgs e)
        {
            if (selection == null)
            {
                lblStatus.Text = "Please select a series first";
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            result = DialogResult.Cancel;
            this.Close();
        }


        public WebScraperInputMatch Selection
        {
            get { return selection; }
            set { selection = value; }
        }

        private void lvSearchResult_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {

        }

        private void lvSearchResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSearchResult.SelectedItems.Count == 1)
            {
                selection = (WebScraperInputMatch)lvSearchResult.SelectedItems[0].Tag;
                //bcSearchResult.BannerImage = m_selection.Banner;

                //txtOverview.Text = m_selection.Overview;
                if (selection.ImdbId != null)
                {
                    linkImdb.Text = selection.ImdbId.Equals("") ? "" : "http://www.imdb.com/title/" + selection.ImdbId;
                }
                txtFirstAired.Text = selection.FirstAired.ToShortDateString();
                txtOverview.Text = selection.Description;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void linkImdb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkImdb.Text);
        }



        internal void UpdateRequest(WebScraperInputRequest _request)
        {
            if (request.Id.Equals(_request.Id) && mNeedsRefresh)
            {
                request = _request;
                FillSearchResults();
                mNeedsRefresh = false;
            }
        }
    }
}
