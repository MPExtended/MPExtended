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
        private IScraperService _service;
        private WebScraper _scraper;
        private DialogResult _result = DialogResult.Cancel;
        private WebScraperInputRequest _request = null;
        private WebScraperInputMatch _selection = null;
        private bool _needsRefresh;
        public String NewSearchText { get; set; }

        public WebScraperInputMatch SelectedMatch { get { return _selection; } }
        public SearchResultForm()
        {
            InitializeComponent();
        }

        public SearchResultForm(WebScraperInputRequest _req)
            : this()
        {
            _request = _req;
            FillSearchResults();
        }

        public SearchResultForm(IScraperService _scraper)
            : this()
        {
            _service = _scraper;
            FillSearchResults();
        }

        public SearchResultForm(IScraperService service, WebScraper scraper, WebScraperInputRequest req)
            : this()
        {
            _service = service;
            _scraper = scraper;
            _request = req;
            FillSearchResults();
        }

        private void FillSearchResults()
        {
            lvSearchResult.Items.Clear();
            if (_request != null && _request.InputOptions != null && _request.InputOptions.Count > 0)
            {
                for (int i = 0; i < _request.InputOptions.Count; i++)
                {
                    ListViewItem item = new ListViewItem(_request.InputOptions[i].ImdbId);
                    item.SubItems.Add(_request.InputOptions[i].Title);
                    item.Tag = _request.InputOptions[i];
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
            _service.SetScraperInputRequest(_scraper.ScraperId, _request.Id, null, txtSeriesName.Text);
            _needsRefresh = true;
            lvSearchResult.Items.Clear();
        }

        private void cmdChoose_Click(object sender, EventArgs e)
        {
            if (_selection == null)
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
            _result = DialogResult.Cancel;
            this.Close();
        }


        public WebScraperInputMatch Selection
        {
            get { return _selection; }
            set { _selection = value; }
        }

        private void lvSearchResult_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {

        }

        private void lvSearchResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSearchResult.SelectedItems.Count == 1)
            {
                _selection = (WebScraperInputMatch)lvSearchResult.SelectedItems[0].Tag;
                //bcSearchResult.BannerImage = m_selection.Banner;

                //txtOverview.Text = m_selection.Overview;
                if (_selection.ImdbId != null)
                {
                    linkImdb.Text = _selection.ImdbId.Equals("") ? "" : "http://www.imdb.com/title/" + _selection.ImdbId;
                }
                txtFirstAired.Text = _selection.FirstAired.ToShortDateString();
                txtOverview.Text = _selection.Description;
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
            if (_request.Id.Equals(_request.Id) && _needsRefresh)
            {
                _request = _request;
                FillSearchResults();
                _needsRefresh = false;
            }
        }
    }
}
