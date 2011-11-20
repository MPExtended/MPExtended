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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabAbout.xaml
    /// </summary>
    public partial class TabAbout : Page
    {
        private BackgroundWorker versionChecker;

        public TabAbout()
        {
            InitializeComponent();
            versionChecker = new BackgroundWorker();
            versionChecker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                string text = String.Format("You're currently using version {0}. ", VersionUtil.GetVersionName());
                if (!UpdateChecker.IsWorking())
                {
                    text += "Failed to retrieve update information.";
                } 
                else if (UpdateChecker.IsUpdateAvailable())
                {
                    text += String.Format("Update available: version {0}, released on {1:dd MMM yyyy}.",
                        UpdateChecker.GetLastReleasedVersion().Version, UpdateChecker.GetLastReleasedVersion().ReleaseDate);
                }
                else
                {
                    text += "No update available.";
                }

                args.Result = text;
            };
            versionChecker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                lblVersion.Content = args.Result as string;
            };
            versionChecker.RunWorkerAsync();
        }
    }
}
