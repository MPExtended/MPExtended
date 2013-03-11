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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Strings;
using MPExtended.Libraries.Service.Util;
using MPExtended.Applications.ServiceConfigurator.Code;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabProject.xaml
    /// </summary>
    public partial class TabProject : Page
    {
        private class UpdateStatus
        {
            public bool Succeeded { get; set; }
            public bool UpdateAvailable { get; set; }
            public ReleasedVersion LastVersion { get; set; }
        }

        private BackgroundWorker versionChecker;

        public TabProject()
        {
            InitializeComponent();
            lblVersion.Text = VersionUtil.GetVersionName();
        }

        private void hbUpdates_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            tbVersion.Text = String.Format("{0} ({1})", VersionUtil.GetVersionName(), UI.CheckingForUpdates);
            e.Handled = true;

            versionChecker = new BackgroundWorker();
            versionChecker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                var result = new UpdateStatus();
                result.Succeeded = UpdateChecker.IsWorking();
                if (result.Succeeded)
                {
                    result.UpdateAvailable = UpdateChecker.IsUpdateAvailable();
                    result.LastVersion = UpdateChecker.GetLastReleasedVersion();
                }
                args.Result = result;
            };
            versionChecker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                var result = (UpdateStatus)args.Result;

                string text;
                if (!result.Succeeded)
                {
                    text = UI.FailedToRetrieveUpdateInformation;
                }
                else if (result.UpdateAvailable)
                {
                    text = String.Format(UI.UpdateAvailable, result.LastVersion.Version, result.LastVersion.ReleaseDate.ToShortDateString());
                }
                else
                {
                    text = UI.NoUpdateAvailable;
                }

                tbVersion.Text = String.Format("{0} ({1})", VersionUtil.GetVersionName(), text);
            };
            versionChecker.RunWorkerAsync();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            CommonEventHandlers.NavigateHyperlink(sender, e);
        }
    }
}

