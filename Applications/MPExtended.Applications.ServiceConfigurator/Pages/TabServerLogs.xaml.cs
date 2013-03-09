#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabServerLogs.xaml
    /// </summary>
    public partial class TabServerLogs : Page, ITabCloseCallback
    {
        // If we just add simple strings to the log viewer, it always scrolls to the _first_ instance of this string in the
        // list items, not to the last one, which is what we want. Instead make sure that all items in the collection are unique,
        // so it always picks the correct line.
        private class LogLine
        {
            public int Id { get; set; }
            public string Text { get; set; }

            public LogLine(int id, string text)
            {
                Id = id;
                Text = text;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private StreamReader mLogStreamReader = null;
        private long lastMaxOffset;
        private string mSelectedLog;
        private DispatcherTimer mLogUpdater;
        private int lastListViewId = 0;

        public TabServerLogs()
        {
            InitializeComponent();

            mLogUpdater = new DispatcherTimer();
            mLogUpdater.Interval = TimeSpan.FromSeconds(2);
            mLogUpdater.Tick += logUpdater_Tick;

            foreach (string file in Directory.GetFiles(Installation.GetLogDirectory()))
            {
                cbLogFiles.Items.Add(Path.GetFileName(file));
            }

            if (cbLogFiles.Items.Count > 0)
            {
                cbLogFiles.SelectedIndex = 0;
                LoadFile((string)cbLogFiles.SelectedItem);
            }
        }

        private void cbLogFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFile((string)cbLogFiles.SelectedItem);
        }

        private void LoadFile(string _file)
        {
            mSelectedLog = _file;
            if (mSelectedLog == null)
            {
                return;
            }

            lvLogViewer.Items.Clear();
            string fullpath = Path.Combine(Installation.GetLogDirectory(), mSelectedLog);

            if (!File.Exists(fullpath))
            {
                Log.Warn("Selected non-existing file {0}", fullpath);
                return;
            }

            // load all the current items
            try
            {
                using (FileStream file = File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader re = new StreamReader(file, Encoding.UTF8))
                    {
                        string input = null;
                        while ((input = re.ReadLine()) != null)
                        {
                            lvLogViewer.Items.Add(new LogLine(lastListViewId++, input));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandling.ShowError(ex);
            }
            ScrollToLastItem(lvLogViewer);

            // start updater at the end of the file
            mLogStreamReader = new StreamReader(new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            lastMaxOffset = mLogStreamReader.BaseStream.Length;
            mLogUpdater.Start();
        }

        public void TabClosed()
        {
            if (mLogUpdater != null)
            {
                mLogUpdater.Stop();
            }

            if (mLogStreamReader != null)
            {
                mLogStreamReader.Close();
            }
        }

        private delegate void AddLogDelegate(string line);
        private void logUpdater_Tick(object sender, EventArgs e)
        {
            // If the file size has not changed, idle
            if (mLogStreamReader.BaseStream.Length == lastMaxOffset)
            {
                return;
            }

            // Seek to the last max offset
            mLogStreamReader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

            // Read out of the file until the EOF
            string readLine = "";
            while ((readLine = mLogStreamReader.ReadLine()) != null)
            {
                // Add to the list view in the UI thread
                lvLogViewer.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (AddLogDelegate)delegate(string line)
                {
                    lvLogViewer.Items.Add(new LogLine(lastListViewId++, line));
                    if (cbLogScrollToEnd.IsChecked == true)
                    {
                        ScrollToLastItem(lvLogViewer);
                    }
                }, readLine);
            }

            //update the last max offset
            lastMaxOffset = mLogStreamReader.BaseStream.Position;
        }

        private void btnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            LogExporter.ExportWithFileChooser();
        }

        public void ScrollToLastItem(ListView lv)
        {
            lv.ScrollIntoView(lv.Items.GetItemAt(lv.Items.Count - 1));
        }
    }
}
