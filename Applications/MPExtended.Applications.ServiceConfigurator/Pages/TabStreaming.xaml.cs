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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabStreaming.xaml
    /// </summary>
    public partial class TabStreaming : Page
    {
        private DispatcherTimer mSessionWatcher;
        private ObservableCollection<WpfStreamingSession> mStreamingSessions = new ObservableCollection<WpfStreamingSession>();

        public TabStreaming()
        {
            // session watcher (started in Page_Initialized, which is called via InitializeComponent)
            mSessionWatcher = new DispatcherTimer();
            mSessionWatcher.Interval = TimeSpan.FromSeconds(2);
            mSessionWatcher.Tick += activeSessionWatcher_Tick;

            InitializeComponent();

            // actually show the list
            lvActiveStreams.ItemsSource = mStreamingSessions;

            // load language list
            var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .Where(x => x.Parent == CultureInfo.InvariantCulture && x.TwoLetterISOLanguageName.Length == 2 && x.TwoLetterISOLanguageName != "iv")
                .GroupBy(x => x.TwoLetterISOLanguageName, (key, items) => items.First())
                .OrderBy(x => x.TwoLetterISOLanguageName)
                .ToDictionary(x => x.TwoLetterISOLanguageName, x => String.Format("{0} ({1})", x.TwoLetterISOLanguageName, x.DisplayName));

            // set valid items
            cbAudio.DataContext = new Dictionary<string, string>() { 
                { "first", "First stream in file" } 
            }.Concat(languages);

            cbSubtitle.DataContext = new Dictionary<string, string>()
            {
                { "none", "Disable subtitles" },
                { "first", "First stream in file" },
                { "external", "Use external .srt file" }
            }.Concat(languages);

            // set default item
            cbAudio.SelectedValue = Configuration.Streaming.DefaultAudioStream;
            cbSubtitle.SelectedValue = Configuration.Streaming.DefaultSubtitleStream;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Streaming.DefaultAudioStream = (string)cbAudio.SelectedValue;
            Configuration.Streaming.DefaultSubtitleStream = (string)cbSubtitle.SelectedValue;
            Configuration.Streaming.Save();
            MessageBox.Show("Saved default language selection!", "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            mSessionWatcher.Start();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended");
            Process.Start(new ProcessStartInfo(folder));
            e.Handled = true;
        }

        private void activeSessionWatcher_Tick(object sender, EventArgs e)
        {
            try
            {
                List<WebStreamingSession> tmp = MPEServices.MASStreamControl.GetStreamingSessions();

                if (tmp != null)
                {
                    mStreamingSessions.UpdateStreamingList(tmp);
                }
            }
            catch (CommunicationException)
            {
                mStreamingSessions.Clear();
                Log.Warn("No connection to service");
            }
        }

        private void miKickUserSession_Click(object sender, RoutedEventArgs e)
        {
            WpfStreamingSession session = (WpfStreamingSession)lvActiveStreams.SelectedItem;
            bool success = MPEServices.MASStreamControl.FinishStream(session.Identifier);
        }
    }
}
