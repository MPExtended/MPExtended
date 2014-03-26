#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Linq;
using System.Text;
using System.ComponentModel;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    public class WpfScraperConfig : WebScraper, INotifyPropertyChanged
    {
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
