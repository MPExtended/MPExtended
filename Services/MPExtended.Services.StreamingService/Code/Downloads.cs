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
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Downloads
    {
        private const int IDLE_TIMEOUT = 10000;

        private class DownloadContext
        {
            public string ClientIP { get; set; }
            public string ClientDescription { get; set; }
            public MediaSource Source { get; set; }
            public DateTime StartTime { get; set; }
            public ReadTrackingStreamWrapper Stream { get; set; }
        }

        private List<DownloadContext> runningDownloads = new List<DownloadContext>();

        public Stream Download(string clientDescription, WebStreamMediaType type, int? provider, string itemId)
        {
            // validate source first
            MediaSource source = new MediaSource(type, provider, itemId);
            if (!source.Exists)
            {
                throw new FileNotFoundException();
            }

            // create context
            DownloadContext context = new DownloadContext()
            {
                ClientDescription = clientDescription, 
                Source = source,
                StartTime = DateTime.Now,
                Stream = new ReadTrackingStreamWrapper(source.Retrieve())
            };

            // see comment in Streaming.cs:151
            string realIp = WCFUtil.GetHeaderValue("forwardedFor", "X-Forwarded-For");
            context.ClientIP = realIp == null ? WCFUtil.GetClientIPAddress() : String.Format("{0} (via {1})", realIp, WCFUtil.GetClientIPAddress());

            // set headers for downloading
            WCFUtil.AddHeader("Content-Disposition", "attachment; filename=\"" + source.GetFileInfo().Name + "\"");
            WCFUtil.SetContentLength(source.GetFileInfo().Size);

            // FIXME: there has to be a better way to do this
            object mime = RegistryReader.ReadKey(Microsoft.Win32.RegistryHive.ClassesRoot, Path.GetExtension(source.GetFileInfo().Name), "Content Type");
            if (mime != null)
            {
                WCFUtil.SetContentType(mime.ToString());
            }

            // finally, save the context and return
            runningDownloads.Add(context);
            return context.Stream;
        }

        public IEnumerable<WebStreamingSession> GetActiveSessions()
        {
            lock (runningDownloads)
            {
                runningDownloads.RemoveAll(x => x.Stream.IsClosed);
            }

            return runningDownloads
                .Where(context => context.Stream.TimeSinceLastRead < IDLE_TIMEOUT)
                .Select(context => new WebStreamingSession()
                {
                    ClientDescription = String.IsNullOrEmpty(context.ClientDescription) ? "Download" : context.ClientDescription,
                    ClientIPAddress = context.ClientIP,
                    DisplayName = context.Source.GetMediaDisplayName(),
                    Identifier = String.Empty,
                    PlayerPosition = 0,
                    Profile = String.Empty,
                    SourceId = context.Source.Id,
                    SourceType = context.Source.MediaType,
                    StartPosition = 0,
                    StartTime = context.StartTime,
                    TranscodingInfo = null
                });
        }
    }
}
