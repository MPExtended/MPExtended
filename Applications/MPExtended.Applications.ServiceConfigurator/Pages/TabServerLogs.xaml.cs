#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabServerLogs.xaml
    /// </summary>
    public partial class TabServerLogs : Page
    {
        public TabServerLogs()
        {
            InitializeComponent();
        }

        private StreamReader mLogStreamReader = null;
        private long lastMaxOffset;
        private string mSelectedLog;
        private DispatcherTimer mLogUpdater;

        private void InitLogTab(String _file)
        {
            mSelectedLog = _file;
            if (mSelectedLog != null)
            {
                lvLogViewer.Items.Clear();
                LoadLogFiles(mSelectedLog);

                String fileName = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", mSelectedLog);
                mLogStreamReader = new StreamReader(new FileStream(fileName,
                         FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                //start at the end of the file
                lastMaxOffset = mLogStreamReader.BaseStream.Length;

                mLogUpdater.Start();
            }
        }

        private void StopLogUpdater()
        {
            mLogUpdater.Stop();
            if (mLogStreamReader != null)
            {
                mLogStreamReader.Close();
            }
        }

        private delegate void AddLogDelegate(String _line);
        private void logUpdater_Tick(object sender, EventArgs e)
        {
            //if the file size has not changed, idle
            if (mLogStreamReader.BaseStream.Length == lastMaxOffset)
            {
                return;
            }

            //seek to the last max offset
            mLogStreamReader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

            //read out of the file until the EOF
            string line = "";
            while ((line = mLogStreamReader.ReadLine()) != null)
            {
                lvLogViewer.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (AddLogDelegate)delegate(String _line)
                {
                    lvLogViewer.Items.Add(_line);
                    if (cbLogScrollToEnd.IsChecked == true)
                    {
                        ScrollToLastItem(lvLogViewer);
                    }
                }, line);
            }
            //update the last max offset
            lastMaxOffset = mLogStreamReader.BaseStream.Position;
        }

        private void btnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".log";
            dlg.Filter = "Log file (.log)|*.log";
            if (dlg.ShowDialog() == true)
            {
                String logFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", mSelectedLog);

                string newFileName = dlg.FileName;

                File.Copy(logFile, newFileName);
            }
        }

        private void LoadLogFiles(string fileName)
        {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", fileName)))
            {
                try
                {
                    StreamReader re = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + String.Format(@"\MPExtended\Logs\{0}.log", fileName));
                    string input = null;
                    while ((input = re.ReadLine()) != null)
                    {
                        lvLogViewer.Items.Add(input);
                    }
                }
                catch (Exception ex)
                {
                    //ExceptionMessageBox(ex.Message);
                }

                //scroll to last item
                ScrollToLastItem(lvLogViewer);
            }
        }

        public void ScrollToLastItem(ListView lv)
        {

            lv.SelectedItem = lv.Items.GetItemAt(lv.Items.Count - 1);
            lv.ScrollIntoView(lv.SelectedItem);
            ListViewItem item = lv.ItemContainerGenerator.ContainerFromItem(lv.SelectedItem) as ListViewItem;
            if (item != null)
            {
                item.Focus();
            }
        }

        private void InitLogFiles()
        {
            String logRoot = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs");

            if (File.Exists(System.IO.Path.Combine(logRoot, "Service.log")))
            {
                cbLogFiles.Items.Add("Service");
            }

            if (cbLogFiles.Items.Count > 0)
            {
                cbLogFiles.SelectedIndex = 0;
                mSelectedLog = (String)cbLogFiles.SelectedItem;
            }
        }
    }
}
