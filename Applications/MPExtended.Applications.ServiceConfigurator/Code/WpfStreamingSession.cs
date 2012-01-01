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
using System.Linq;
using System.Text;
using System.Windows;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class WpfStreamingSession : INotifyPropertyChanged
    {
        private WebStreamingSession mStreamingSession;

        public WpfStreamingSession(WebStreamingSession sNew)
        {
            UpdateStreamingSession(sNew);
        }

        public void UpdateStreamingSession(WebStreamingSession newSession)
        {
            mStreamingSession = newSession;
            this.Identifier = newSession.Identifier;
            this.ClientDescription = newSession.ClientDescription;
            this.ClientIP = newSession.ClientIPAddress;

            if (this.Profile == null || !this.Profile.Equals(newSession.Profile))
            {
                this.Profile = newSession.Profile;
                NotifyPropertyChanged("Profile");
            }

            String file = newSession.DisplayName;
            if (this.File == null || !this.File.Equals(file))
            {
                this.File = file;
                NotifyPropertyChanged("File");
            }

            if (newSession.PlayerPosition > 0)
            {
                TimeSpan span = TimeSpan.FromMilliseconds(newSession.PlayerPosition);
                span = span.Subtract(TimeSpan.FromMilliseconds(span.Milliseconds)); // don't show the damn milliseconds
                String progress = span.ToString("g");
                if (this.Progress == null || !this.Progress.Equals(progress))
                {
                    this.Progress = progress;
                    NotifyPropertyChanged("Progress");
                }
            }
        }

        public String Identifier { get; set; }
        public String ClientDescription { get; set; }
        public String ClientIP { get; set; }
        public String Profile { get; set; }
        public String File { get; set; }
        public String Progress { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
