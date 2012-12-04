using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.ScraperService.Interfaces;
using System.IO;
using MPExtended.Libraries.Service;

namespace MPExtended.Scrapers.ItemDownloader
{
    internal class ItemDownloadJob
    {
        internal bool IsDownloading { get; set; }
        internal FileInfo JobFile { get; set; }

        internal String MpExtId { get; set; }
        internal int? MpExtProviderId { get; set; }
        internal WebMediaType MpExtItemType { get; set; }
        internal long ResumePosition { get; set; }

        internal WebScraperItem ScraperItem { get; set; }

        internal void Update(string state, int progress)
        {
            ScraperItem.Progress = progress;
            Update(state);
        }

        internal void Update(string state)
        {
            ScraperItem.State = state;
            ScraperItem.LastUpdated = DateTime.Now;
        }



        internal void DeleteTemporaryDownload()
        {
            if (JobFile != null && File.Exists(JobFile.FullName))
            {
                try
                {
                    JobFile.Delete();
                }
                catch (Exception ex)
                {
                    Log.Warn("Couldn't delete temporary dowload file", ex);
                }
            }
        }
    }
}
