using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.ScraperService.Interfaces;
using System.Threading;
using System.IO;
using MPExtended.Libraries.Client;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Scrapers.ItemDownloader
{
    public class ItemDownloader : IScraperPlugin
    {
        private Thread _downloadThread;
        private WebScraperState _state;
        private List<ItemDownloadJob> _pendingDownloads = new List<ItemDownloadJob>();
        private List<ItemDownloadJob> _finishedDownloads = new List<ItemDownloadJob>();
        private IServiceSet _services;
        private String Root = @"c:\\Users\DieBagger\\Downloads\\";
        private String _server;
        private String _user;
        private String _password;

        private bool _pauseCurrentDownload = false;
        private bool _cancelCurrentDownload = false;

        private List<WebScraperAction> _downloadActions;


        public ItemDownloader(String server, String user, String pwd)
        {
            _server = server;
            _user = user;
            _password = pwd;
            _state = WebScraperState.Stopped;

            _downloadActions = new List<WebScraperAction>();
            _downloadActions.Add(new WebScraperAction() { ActionId = "action_pause", Title = "Pause", Description = "Pause this download", Enabled = true });
            _downloadActions.Add(new WebScraperAction() { ActionId = "action_cancel", Title = "Cancel", Description = "Cancel this download", Enabled = true });
            _downloadActions.Add(new WebScraperAction() { ActionId = "action_start", Title = "Start", Description = "Start or resume this download", Enabled = true });
        }

        #region IPrivateScraperService interface
        public WebScraper GetScraperDescription()
        {
            return new WebScraper()
            {
                ScraperId = 2,
                ScraperName = "Downloader"
            };
        }

        public WebResult StartScraper()
        {
            if (_state == WebScraperState.Stopped)
            {
                _services = new ServiceAddressSet(_server, null).Connect(_user, _password);
                _state = WebScraperState.Running;
                _downloadThread = new Thread(new ThreadStart(DownloaderThread));
                _downloadThread.Start();
                return true;
            }
            else
            {
                //already running
                return false;
            }
        }

        public WebResult StopScraper()
        {
            if (_state != WebScraperState.Stopped)
            {
                _state = WebScraperState.Stopped;
                return true;
            }
            else
            {
                //already running
                return false;
            }
        }

        public WebResult PauseScraper()
        {
            if (_state == WebScraperState.Running)
            {
                _state = WebScraperState.Paused;
                return true;
            }
            else
            {
                //already running
                return false;
            }
        }

        public WebResult ResumeScraper()
        {
            if (_state == WebScraperState.Paused)
            {
                _state = WebScraperState.Running;
                return true;
            }
            else
            {
                //already running
                return false;
            }
        }

        public WebResult TriggerUpdate()
        {
            return true;
        }

        public WebScraperStatus GetScraperStatus()
        {
            return new WebScraperStatus()
            {
                ScraperState = _state
            };
        }

        public WebScraperInputRequest GetScraperInputRequest(int index)
        {
            return null;
        }

        public IList<WebScraperInputRequest> GetAllScraperInputRequests()
        {
            return null;
        }

        public WebResult SetScraperInputRequest(string requestId, string matchId, string text)
        {
            return false;
        }

        public WebResult AddItemToScraper(String title, WebMediaType type, int? provider, string itemId, int? offset)
        {
            String jobId = new Random((int)DateTime.Now.Ticks).Next(0, 100000).ToString() + "_" + itemId;

            ItemDownloadJob job = new ItemDownloadJob()
            {
                ScraperItem = new WebScraperItem()
                {
                    ItemId = jobId,
                    Title = title,
                    ItemActions = _downloadActions,
                    Progress = -1
                },
                MpExtId = itemId,
                MpExtItemType = type,
                MpExtProviderId = provider
            };

            job.Update("Pending", 0);
            _pendingDownloads.Add(job);

            return true;
        }
        #endregion

        private void DownloaderThread()
        {
            while (_state != WebScraperState.Stopped)
            {
                ItemDownloadJob job = null;
                if (_state == WebScraperState.Running && _pendingDownloads.Count > 0)
                {
                    job = _pendingDownloads[0];

                    DownloadItem(job);

                    job.IsDownloading = false;

                    _finishedDownloads.Add(job);
                    _pendingDownloads.Remove(job);
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private bool DownloadItem(ItemDownloadJob job)
        {
            try
            {
                job.IsDownloading = true;
                _pauseCurrentDownload = false;
                _cancelCurrentDownload = false;

                Log.Info("Starting download of " + job.ScraperItem.Title);

                WebMediaItem item = _services.MAS.GetMediaItem(job.MpExtProviderId, job.MpExtItemType, job.MpExtId);

                if (item != null)
                {
                    WebFileInfo fileInfo = _services.MAS.GetFileInfo(job.MpExtProviderId, job.MpExtItemType, Services.Common.Interfaces.WebFileType.Content, job.MpExtId, 0);

                    job.ScraperItem.Title = fileInfo.Name;
                    job.Update("Downloading");

                    _services.MASStreamControl.AuthorizeStreaming();
                    FileInfo file = new FileInfo(item.Path[0]);

                    if (job.JobFile == null)
                    {
                        //new download
                        FileInfo local = new FileInfo(Root + file.Name);

                        if (local.Exists)
                        {
                            for (int i = 1; i < 10000; i++)
                            {
                                String newFile = local.FullName.Substring(0, local.FullName.Length - local.Extension.Length) + " - " + i + local.Extension;
                                if (!File.Exists(newFile))
                                {
                                    local = new FileInfo(newFile);
                                    break;
                                }
                            }
                        }

                        job.JobFile = local;
                    }

                    if (job.JobFile.Exists)
                    {
                        job.ResumePosition = job.JobFile.Length;
                    }
                    else
                    {
                        job.ResumePosition = 0;
                    }

                    FileStream fileStream = new FileStream(job.JobFile.FullName, FileMode.OpenOrCreate);
                    fileStream.Position = fileStream.Length;

                    Stream mediaStream = _services.MASStream.GetMediaItem("ItemDownloader", job.MpExtItemType, job.MpExtProviderId, job.MpExtId, job.ResumePosition);

                    byte[] buffer = new byte[8 * 1024];
                    int len;
                    long size = 0;
                    int percentageDone = CalculatePercentage(job.ResumePosition, fileInfo.Size);
                    while ((len = mediaStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, len);
                        size += len;

                        int newPercentage = CalculatePercentage(job.ResumePosition + size, fileInfo.Size);
                        if (newPercentage > percentageDone)
                        {
                            job.Update("Downloading", newPercentage);
                            Log.Info("Downloading " + job.JobFile.Name + ": " + newPercentage + " %");
                        }
                        percentageDone = newPercentage;
                        job.ResumePosition += len;

                        if (_state != WebScraperState.Running || _pauseCurrentDownload)
                        {
                            job.Update("Paused");
                            break;
                        }

                        if (_cancelCurrentDownload)
                        {
                            job.Update("Cancelled");
                            break;
                        }
                    }
                    fileStream.Close();

                    if (_cancelCurrentDownload)
                    {
                        job.DeleteTemporaryDownload();
                    }

                    if (job.ResumePosition >= fileInfo.Size)
                    {
                        job.Update("Finished");
                        return true;
                    }
                }
                else
                {
                    job.Update("Failed");
                    Log.Warn("Item not found on server");
                }
            }
            catch (Exception ex)
            {
                job.Update("Failed");
                Log.Warn("Error on download", ex);
            }

            return false;
        }

        private int CalculatePercentage(long position, long size)
        {
            if (size == 0)
                return 0;
            return (int)(position * 100 / size);
        }


        public List<WebScraperItem> GetScraperItems()
        {
            return _pendingDownloads.Concat(_finishedDownloads).Select(e => e.ScraperItem).ToList();
        }

        public List<WebScraperAction> GetScraperActions()
        {
            return new List<WebScraperAction>();
        }

        /// <summary>
        /// Invoke a scraper action
        /// </summary>
        /// <param name="itemId">Item where the action is invoked on</param>
        /// <param name="actionId">Id of the action</param>
        /// <returns></returns>
        public WebBoolResult InvokeScraperAction(string itemId, string actionId)
        {
            foreach (ItemDownloadJob j in _pendingDownloads.Concat(_finishedDownloads))
            {
                if (j.ScraperItem.ItemId == itemId)
                {
                    if (actionId == "action_cancel")
                    {
                        CancelDownload(j);
                        return true;
                    }
                    else if (actionId == "action_pause")
                    {
                        PauseDownload(j);
                        return true;
                    }
                    else if (actionId == "action_start")
                    {
                        StartDownload(j);
                        return true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Pause the download of a download item
        /// </summary>
        /// <param name="job">job to cancel</param>
        private void PauseDownload(ItemDownloadJob job)
        {
            job.Update("Paused");
            if (job.IsDownloading)
            {
                _pauseCurrentDownload = true;
            }
            else
            {
                _pendingDownloads.Remove(job);
            }
        }

        /// <summary>
        /// Start/Resume the download of a download item
        /// </summary>
        /// <param name="job">job to start</param>
        private void StartDownload(ItemDownloadJob job)
        {
            if (_finishedDownloads.Contains(job))
            {
                _finishedDownloads.Remove(job);
                job.Update("Queued");
                _pendingDownloads.Add(job);
            }
        }

        /// <summary>
        /// Cancel the download job
        /// </summary>
        /// <param name="job">job to cancel</param>
        private void CancelDownload(ItemDownloadJob job)
        {
            job.Update("Cancelled");
            if (job.IsDownloading)
            {
                _cancelCurrentDownload = true;
            }
            else
            {
                job.DeleteTemporaryDownload();
                _pendingDownloads.Remove(job);
            }
        }


        public WebBoolResult ShowConfig()
        {
            throw new NotImplementedException();
        }
    }
}
