#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Client;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.Development.FillMediaInfoCache
{
    public partial class frmFillCache : Form
    {
        private IServiceSet _services;

        private BackgroundWorker bwMovies;
        private BackgroundWorker bwEpisodes;
        private bool isRunning = false;

        private int movieCount;
        private int episodeCount;

        public frmFillCache()
        {
            InitializeComponent();
        }

        private void frmFillCache_Load(object sender, EventArgs e)
        {
            _services = new ServiceAddressSet("127.0.0.1", "127.0.0.1").Connect();

            movieCount = _services.MAS.GetMovieCount(null);
            episodeCount = _services.MAS.GetTVEpisodeCount(null);

            lblMovies.Text = "Movies (0 / " + movieCount + "):";
            prgMovies.Maximum = movieCount;
            lblEpisodes.Text = "Episodes (0 / " + episodeCount + "):";
            prgEpisodes.Maximum = episodeCount;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                bwMovies.CancelAsync();
                bwEpisodes.CancelAsync();

                isRunning = false;
                btnStart.Text = "Start";
            }
            else
            {
                bwMovies = new BackgroundWorker();
                bwMovies.WorkerReportsProgress = true;
                bwMovies.WorkerSupportsCancellation = true;
                bwMovies.ProgressChanged += delegate(object bwSender, ProgressChangedEventArgs bwE)
                {
                    lblMovies.Text = "Movies (" + bwE.UserState + " / " + movieCount + "):";
                    prgMovies.Value = (int)bwE.UserState;
                };
                bwMovies.DoWork += delegate(object bwSender, DoWorkEventArgs bwEvent)
                {
                    int j = 0;
                    BackgroundWorker worker = (BackgroundWorker)bwSender;
                    for (int i = 0; i < movieCount; i += 100)
                    {
                        var movies = _services.MAS.GetMoviesBasicByRange(null, i, i + 99);
                        foreach (var movie in movies)
                        {
                            _services.MASStreamControl.GetMediaInfo(WebMediaType.Movie, movie.PID, movie.Id, 0);
                            worker.ReportProgress(5, ++j);
                            if (worker.CancellationPending)
                            {
                                bwEvent.Cancel = true;
                                break;
                            }
                        }

                        if (bwEvent.Cancel)
                            break;
                    }
                };
                bwMovies.RunWorkerCompleted += delegate(object bwSender, RunWorkerCompletedEventArgs bwEvent)
                {
                    if (!bwEpisodes.IsBusy)
                    {
                        isRunning = false;
                        btnStart.Text = "Start";
                    }
                };
                bwMovies.RunWorkerAsync();

                bwEpisodes = new BackgroundWorker();
                bwEpisodes.WorkerReportsProgress = true;
                bwEpisodes.WorkerSupportsCancellation = true;
                bwEpisodes.ProgressChanged += delegate(object bwSender, ProgressChangedEventArgs bwE)
                {
                    lblEpisodes.Text = "Episodes (" + bwE.UserState + " / " + episodeCount + "):";
                    prgEpisodes.Value = (int)bwE.UserState;
                };
                bwEpisodes.DoWork += delegate(object bwSender, DoWorkEventArgs bwEvent)
                {
                    int j = 0;
                    BackgroundWorker worker = (BackgroundWorker)bwSender;
                    for (int i = 0; i < episodeCount; i += 100)
                    {
                        var episodes = _services.MAS.GetTVEpisodesBasicByRange(null, i, i + 99);
                        foreach (var episode in episodes)
                        {
                            _services.MASStreamControl.GetMediaInfo(WebMediaType.TVEpisode, episode.PID, episode.Id, 0);
                            worker.ReportProgress(5, ++j);
                            if (worker.CancellationPending)
                            {
                                bwEvent.Cancel = true;
                                break;
                            }
                        }

                        if (bwEvent.Cancel)
                            break;
                    }
                };
                bwEpisodes.RunWorkerCompleted += delegate(object bwSender, RunWorkerCompletedEventArgs bwEvent)
                {
                    if (!bwMovies.IsBusy)
                    {
                        isRunning = false;
                        btnStart.Text = "Start";
                    }
                };
                bwEpisodes.RunWorkerAsync();

                btnStart.Text = "Stop";
                isRunning = true;
            }
        }
    }
}
