using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.StreamingService.Interfaces;
using System.ComponentModel;
using System.Windows;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    public class WpfStreamingSession : INotifyPropertyChanged
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

            String progress = newSession.TranscodingInfo != null ? newSession.TranscodingInfo.CurrentTime.ToString() : "";
            if (this.Progress == null || !this.Progress.Equals(progress))
            {
                this.Progress = progress;
                NotifyPropertyChanged("Progress");
            }

        }

        public String Identifier { get; set; }
        public String ClientDescription { get; set; }
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
