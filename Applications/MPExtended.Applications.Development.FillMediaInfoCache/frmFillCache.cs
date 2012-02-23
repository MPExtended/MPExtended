#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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

namespace MPExtended.Applications.Development.FillMediaInfoCache
{
    public partial class frmFillCache : Form
    {
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
            movieCount = MPEServices.MAS.GetMovieCount(null).Count;
            episodeCount = MPEServices.MAS.GetTVEpisodeCount(null).Count;

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
                bwMovies.DoWork += delegate(object bwSender, DoWorkEventArgs bwE)
                {
                    int j = 0;
                    BackgroundWorker worker = (BackgroundWorker)bwSender;
                    for (int i = 0; i < movieCount; i += 100)
                    {
                        var movies = MPEServices.MAS.GetMoviesBasicByRange(null, i, i + 100);
                        foreach (var movie in movies)
                        {
                            MPEServices.MASStreamControl.GetMediaInfo(WebStreamMediaType.Movie, movie.PID, movie.Id);
                            worker.ReportProgress(5, ++j);
                            if (worker.CancellationPending)
                            {
                                bwE.Cancel = true;
                                goto outMovies;
                            }
                        }
                    }
outMovies:
                    ;
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
                bwEpisodes.DoWork += delegate(object bwSender, DoWorkEventArgs bwE)
                {
                    int j = 0;
                    BackgroundWorker worker = (BackgroundWorker)bwSender;
                    for (int i = 0; i < episodeCount; i += 100)
                    {
                        var episodes = MPEServices.MAS.GetTVEpisodesBasicByRange(null, i, i + 100);
                        foreach (var episode in episodes)
                        {
                            MPEServices.MASStreamControl.GetMediaInfo(WebStreamMediaType.TVEpisode, episode.PID, episode.Id);
                            worker.ReportProgress(5, ++j);
                            if (worker.CancellationPending)
                            {
                                bwE.Cancel = true;
                                goto outEpisodes;
                            }
                        }
                    }
outEpisodes:
                    ;
                };
                bwEpisodes.RunWorkerAsync();

                btnStart.Text = "Stop";
                isRunning = true;
            }
        }

        void bwMovies_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
