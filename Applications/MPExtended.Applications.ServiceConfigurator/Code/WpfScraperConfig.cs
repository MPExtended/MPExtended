using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    public class WpfScraperConfig : WebScraper, INotifyPropertyChanged
    {
        private WebScraper _scraper;

        public WpfScraperConfig() { }

        public WpfScraperConfig(WebScraper old)
        {
            UpdateScraper(old);
        }

        public void UpdateScraper(WebScraper newScraper)
        {
            ScraperId = newScraper.ScraperId;
            ScraperName = newScraper.ScraperName;
            ScraperDescription = newScraper.ScraperDescription;

            if (this.ScraperInfo == null || !this.ScraperInfo.Equals(newScraper.ScraperInfo))
            {
                this.ScraperInfo = newScraper.ScraperInfo;
                NotifyPropertyChanged("ScraperInfo");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
