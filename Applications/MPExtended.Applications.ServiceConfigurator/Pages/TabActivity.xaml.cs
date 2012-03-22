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
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabStreaming.xaml
    /// </summary>
    public partial class TabActivity : Page, ITabCloseCallback
    {
        private DispatcherTimer mSessionWatcher;
        private ObservableCollection<WpfStreamingSession> mStreamingSessions = new ObservableCollection<WpfStreamingSession>();

        public TabActivity()
        {
            // session watcher (started in Page_Initialized, which is called via InitializeComponent)
            mSessionWatcher = new DispatcherTimer();
            mSessionWatcher.Interval = TimeSpan.FromSeconds(2);
            mSessionWatcher.Tick += activeSessionWatcher_Tick;

            InitializeComponent();

            // actually show the list
            lvActiveStreams.ItemsSource = mStreamingSessions;

            // start observing
            mSessionWatcher.Start();
        }


        public void TabClosed()
        {
            mSessionWatcher.Stop();
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
            if (session != null)
            {
                bool success = MPEServices.MASStreamControl.FinishStream(session.Identifier);
            }
        }
    }
}
