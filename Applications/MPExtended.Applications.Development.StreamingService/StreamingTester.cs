#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.Development.StreamingService
{
    public partial class Form1 : Form
    {
        private const string CURRENT_IP = "localhost";
        private const string CLIENT_NAME = "StreamingService TestTool";
        private const string VLC_PATH = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";

        private IMediaAccessService mServiceClient;
        private ITVAccessService mTvClient;
        private IWebStreamingService mWebStreamClient;
        private IStreamingService mStreamClient;

        private IList<WebMovieDetailed> mMovies;
        private IList<WebChannelBasic> mChannels;
        private IList<WebTranscoderProfile> mProfiles;
        private string mIdentifier;
        private string mName;
        private Thread mDlThread;
        private WebMediaInfo mInfo;

        public FileStream mCurrentFile;

        private int fileProvider;
        private int movieProvider;

        public Form1()
        {
            InitializeComponent();

            cbItemType.Items.Add(WebStreamMediaType.File);
            cbItemType.Items.Add(WebStreamMediaType.Movie);
            cbItemType.Items.Add(WebStreamMediaType.MusicAlbum);
            cbItemType.Items.Add(WebStreamMediaType.MusicTrack);
            cbItemType.Items.Add(WebStreamMediaType.Picture);
            cbItemType.Items.Add(WebStreamMediaType.Recording);
            cbItemType.Items.Add(WebStreamMediaType.TV);
            cbItemType.Items.Add(WebStreamMediaType.TVEpisode);
            cbItemType.Items.Add(WebStreamMediaType.TVSeason);
            cbItemType.Items.Add(WebStreamMediaType.TVShow);
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {

            if (CURRENT_IP == "127.0.0.1" || CURRENT_IP == "localhost")
            {
                
                mWebStreamClient = ChannelFactory<IWebStreamingService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/StreamingService"));
                mStreamClient = ChannelFactory<IStreamingService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/StreamingService"));
                mServiceClient = ChannelFactory<IMediaAccessService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/MediaAccessService"));
                mTvClient = ChannelFactory<ITVAccessService>.CreateChannel(new NetNamedPipeBinding() { MaxReceivedMessageSize = 10000000 }, new EndpointAddress("net.pipe://localhost/MPExtended/TVAccessService"));
            }
            else
            {
#pragma warning disable 0162
                mWebStreamClient = ChannelFactory<IWebStreamingService>.CreateChannel(new BasicHttpBinding(), new EndpointAddress("http://" + CURRENT_IP + ":4321/MPExtended/StreamingService"));
                mStreamClient = ChannelFactory<IStreamingService>.CreateChannel(new BasicHttpBinding(), new EndpointAddress("http://" + CURRENT_IP + ":4321/MPExtended/StreamingService"));
                mServiceClient = ChannelFactory<IMediaAccessService>.CreateChannel(new BasicHttpBinding(), new EndpointAddress("http://" + CURRENT_IP + ":4321/MPExtended/MediaAccessService"));
                mTvClient = ChannelFactory<ITVAccessService>.CreateChannel(new BasicHttpBinding(), new EndpointAddress("http://" + CURRENT_IP + ":4321/MPExtended/TVAccessService"));
#pragma warning restore 0162
            }

            Log("Initialized");

            // providers
            var config = mServiceClient.GetServiceDescription();
            movieProvider = config.AvailableMovieLibraries.First().Id;
            fileProvider = config.AvailableFileSystemLibraries.First().Id;

            // load movies
            try
            {
                cbMovies.Items.Clear();
                mMovies = mServiceClient.GetAllMoviesDetailed(movieProvider, null, null);
                foreach (WebMovieDetailed movie in mMovies)
                {
                    cbMovies.Items.Add(movie.Title);
                }

                Log("Loaded movies");
            }
            catch (Exception)
            {
                Log("Failed to connect to MAS");
            }

            // load chanels
            try
            {
                cbChannels.Items.Clear();
                mChannels = new List<WebChannelBasic>();
                foreach (WebChannelGroup group in mTvClient.GetGroups())
                {
                    WebChannelBasic[] channels = mTvClient.GetChannelsBasic(group.Id).ToArray();
                    foreach (WebChannelBasic ch in channels)
                    {
                        cbChannels.Items.Add(ch.DisplayName);
                        mChannels.Add(ch);
                    }
                }
                Log("Loaded channels");
            }
            catch (Exception)
            {
                Log("Failed to connect to TV4Home");
            }

            // load profiles
            try
            {
                cbProfiles.Items.Clear();
                mProfiles = mWebStreamClient.GetTranscoderProfiles();
                foreach (WebTranscoderProfile profile in mProfiles)
                {
                    cbProfiles.Items.Add(profile.Name);
                }
                cbProfiles.SelectedIndex = 0;
            }
            catch (Exception)
            {
                Log("Failed to load profiles");
            }
        }

        private void cmdInitMovie_Click(object sender, EventArgs e)
        {
            mIdentifier = "Test_" + new Random().Next(0, 1000000).ToString();
            WebMovieDetailed movie = mMovies[cbMovies.SelectedIndex];
            mName = movie.Title;
            Log("Init Stream with movie " + movie.Title);
            bool success = mWebStreamClient.InitStream(WebStreamMediaType.Movie, movieProvider, movie.Id.ToString(), CLIENT_NAME, mIdentifier, null);
            Log("Success = " + success);
            LoadMediaInfo(mWebStreamClient.GetMediaInfo(WebStreamMediaType.Movie, movieProvider, movie.Id.ToString()));
        }

        private void cmdInitChannel_Click(object sender, EventArgs e)
        {
            mIdentifier = "Test_" + new Random().Next(0, 1000000).ToString();
            WebChannelBasic channel = mChannels[cbChannels.SelectedIndex];
            mName = channel.DisplayName;
            Log("Init Stream with channel " + channel.DisplayName);
            bool success = mWebStreamClient.InitStream(WebStreamMediaType.TV, 0, channel.Id.ToString(), CLIENT_NAME, mIdentifier, null);
            Log("Success = " + success);
            LoadMediaInfo(mWebStreamClient.GetMediaInfo(WebStreamMediaType.TV, 0, mIdentifier));
        }

        private void cmdInitIdTypeStreaming_Click(object sender, EventArgs e)
        {
            int provider = Int32.Parse(txtProvider.Text);
            mIdentifier = "Test_" + new Random().Next(0, 1000000).ToString();
            mName = txtItemId.Text;
            Log("Init Stream with id " + txtItemId.Text + " (type: " + cbItemType.SelectedItem + ")");
            bool success = mWebStreamClient.InitStream((WebStreamMediaType)cbItemType.SelectedItem, provider, txtItemId.Text, CLIENT_NAME, mIdentifier, null);
            Log("Success = " + success);
            Thread.Sleep(500); // wait a bit till the stream has populated
            LoadMediaInfo(mWebStreamClient.GetMediaInfo((WebStreamMediaType)cbItemType.SelectedItem, provider, txtItemId.Text));
        }

        private void LoadMediaInfo(WebMediaInfo info)
        {
            if (info == null)
            {
                Log("No MediaInfo available");
                return;
            }

            mInfo = info;
            Log(String.Format("MediaInfo: streams: {0} video, {1} audio, {2} subtitle", 
                info.VideoStreams.Count, info.AudioStreams.Count, info.SubtitleStreams.Count));
            if (info.VideoStreams.Count() > 0)
            {
                WebVideoStream vid = info.VideoStreams.First();
                Log(String.Format("MediaInfo: video {0}x{1}; {2}; {3}", vid.Width, vid.Height, vid.DisplayAspectRatioString, vid.Codec));
            }

            // audio
            cbLanguage.Items.Clear();
            int i = 1;
            foreach (WebAudioStream audio in info.AudioStreams)
            {
                string name = "Audio stream #" + i++;
                if(!String.IsNullOrEmpty(audio.Language))
                    name += ": " + audio.Language;
                if(!String.IsNullOrEmpty(audio.Title))
                    name += ": " + audio.Title;
                cbLanguage.Items.Add(name);
            }

            // subtitle
            cbSubtitle.Items.Clear();
            i = 1;
            foreach (WebSubtitleStream stream in info.SubtitleStreams)
            {
                string name = "Subtitle stream #" + i++ + " (" + stream.ID + ")";
                if(!String.IsNullOrEmpty(stream.Language))
                    name += ": " + stream.Language;
                cbSubtitle.Items.Add(name);
            }
        }

        private void Log(string _log)
        {
            Invoke((MethodInvoker)delegate
            {
                lbLog.Items.Insert(0, _log);
            });
        }

        private void LogFileSize(long size)
        {
            Invoke((MethodInvoker)delegate
            {
                txtFileSize.Text = size.ToString() ;
            });
        }

        private void cmdFinishStreaming_Click(object sender, EventArgs e)
        {
            mWebStreamClient.FinishStream(mIdentifier);
            Log("Finished stream");
        }

        private void cmdStartStream_Click(object sender, EventArgs e)
        {
            StartAndDownloadStream(0);
        }

        private void StartAndDownloadStream(int _pos)
        {
            int language = cbLanguage.SelectedIndex == -1 ? -1 : mInfo.AudioStreams[cbLanguage.SelectedIndex].ID;
            int subtitle = cbSubtitle.SelectedIndex == -1 ? -1 : mInfo.SubtitleStreams[cbSubtitle.SelectedIndex].ID;

            Log("Starting Stream from pos " + _pos);
            string url = mWebStreamClient.StartStreamWithStreamSelection(mIdentifier, mProfiles[cbProfiles.SelectedIndex].Name, _pos, language, subtitle);
            if (String.IsNullOrEmpty(url))
            {
                Log("StartStream failed");
            }
            else
            {
                DownloadStream(url, _pos);
            }
        }

        private void DownloadStream(string url, int _startPos)
        {
            Log("Retrieve Stream from pos " + _startPos);

            if (!Directory.Exists(mName))
            {
                Directory.CreateDirectory(mName);
            }
            string filename = mName + "\\" + mName.Replace(":", "-") + "_from_" + _startPos + ".ts";

            mDlThread = new Thread(new ThreadStart(delegate()
            {
                using(WebClient cl = new WebClient())
                {
                    cl.Credentials = new NetworkCredential("admin", "admin");
                    FileStream file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    mCurrentFile = file;
                    Stream webstream = cl.OpenRead(url);
                    CopyStream(webstream, file);
                    mCurrentFile = null;
                    file.Close();
                }
            }));

            mDlThread.Start();
        }

        private Stopwatch mStopwatch;
        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            long size = 0;
            mStopwatch = new Stopwatch();
            mStopwatch.Start();
            bool hasStarted = false;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
                size += len;
                if (!hasStarted && size > 50000 && cbStartAutoPlayback.Checked)
                {
                    StartPlaybackInVlc();
                    hasStarted = true;
                }

                if (mStopwatch.ElapsedMilliseconds > 500)
                {
                    LogFileSize(size);
                    mStopwatch.Reset();
                    mStopwatch.Restart();
                }
            }
            Log("Finished reading from media stream");
        }



        private void cmdSeekToPos_Click(object sender, EventArgs e)
        {
            int pos = Int32.Parse(txtStartPos.Text);

            StartAndDownloadStream(pos);
        }

        #region Quick-Seek-Buttons
        private void cmdSeek1000_Click(object sender, EventArgs e)
        {
            int pos = 1000;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek3600_Click(object sender, EventArgs e)
        {
            int pos = 3600;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek500_Click(object sender, EventArgs e)
        {
            int pos = 500;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek3000_Click(object sender, EventArgs e)
        {
            int pos = 3000;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek400_Click(object sender, EventArgs e)
        {
            int pos = 400;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek2500_Click(object sender, EventArgs e)
        {
            int pos = 2500;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek300_Click(object sender, EventArgs e)
        {
            int pos = 300;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek2000_Click(object sender, EventArgs e)
        {
            int pos = 2000;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek200_Click(object sender, EventArgs e)
        {
            int pos = 200;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek1500_Click(object sender, EventArgs e)
        {
            int pos = 1500;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek100_Click(object sender, EventArgs e)
        {
            int pos = 100;
            StartAndDownloadStream(pos);
        }

        private void cmdSeek0_Click(object sender, EventArgs e)
        {
            int pos = 0;
            StartAndDownloadStream(pos);
        }

        #endregion

        private void cmdPlayInVlc_Click(object sender, EventArgs e)
        {
            StartPlaybackInVlc();
        }

        private void StartPlaybackInVlc()
        {
            if (mCurrentFile != null)
            {
                ProcessStartInfo info = new ProcessStartInfo(VLC_PATH, " \"" + mCurrentFile.Name + "\"");
                Process.Start(info);
            }
        }


    }
}
