#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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

// This unit process an MPEG TS input stream and converts it into suitable short segments for streaming. As such, it doesn't 
// have an output stream, but instead a it needs some kind of webserver. This is handled by HTTPLiveTranscoderWrapper and 
// Streaming.cs.
//
// See http://tools.ietf.org/html/draft-pantos-http-live-streaming-06 for the specification of HTTP Live Streaming, but we don't
// deal with the spec directly as we let the tool handle it. 
//
// The tool has some quirks:
// - with SEGMENT_LENGTH = 10 the first 5 files are of length 15, 17, 12, 15 and 7.
//
// VLC 1.2 does support HTTP Live Streaming by itself. However, it isn't anywhere near being ready for a release, so we can't
// use it here yet. This will probably go away after VLC 1.2 release but that'll take a while. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Units
{
    internal class HTTPLiveStreamingUnit : IProcessingUnit
    {
        private const int SEGMENT_LENGTH = 10;
        private const int SEGMENT_BUFFER = 10;

        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        public bool DebugOutput { get; set; }
        public string TemporaryDirectory { get; set; }

        private string identifier;
        private Process segmenterApplication;
        private string siteRoot;

        public HTTPLiveStreamingUnit(string identifier)
        {
            this.identifier = identifier;
        }

        public bool Setup()
        {
            siteRoot = WCFUtil.GetCurrentRoot() + "StreamingService/stream/CustomTranscoderData?identifier=" + identifier + "&action=segment&parameters=";

            TemporaryDirectory = Path.Combine(Path.GetTempPath(), "MPExtended.Services.StreamingService.HTTPLiveStreaming-" + new Random().Next());
            Directory.CreateDirectory(TemporaryDirectory);
            Log.Debug("HTTPLiveStreaming: created temporary directory {0}", TemporaryDirectory);

            string arguments = String.Format("- {0} segment playlist.m3u8 {2} {3}", SEGMENT_LENGTH, TemporaryDirectory, siteRoot, SEGMENT_BUFFER);
            string segmenter = @"..\..\..\..\Libraries\Streaming\segmenter\segmenter.exe";

            ProcessStartInfo start = new ProcessStartInfo(segmenter, arguments);
            start.UseShellExecute = false;
            start.RedirectStandardInput = true;
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.WindowStyle = DebugOutput ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            start.CreateNoWindow = !DebugOutput;
            start.WorkingDirectory = TemporaryDirectory;

            Log.Info("HTTPLiveStreaming: segmenter arguments: {0}", arguments);

            try
            {
                segmenterApplication = new Process();
                segmenterApplication.StartInfo = start;
                segmenterApplication.Start();
            }
            catch (Exception ex)
            {
                Log.Error("HTTPLiveStreaming: failed to start segmenter", ex);
                return false;
            }

            return true;
        }

        public bool Start()
        {
            StreamCopy.AsyncStreamCopy(InputStream, segmenterApplication.StandardInput.BaseStream);

            return true;
        }

        public bool Stop()
        {
            try
            {
                if (segmenterApplication != null && !segmenterApplication.HasExited)
                {
                    Log.Debug("HTTPLiveStreaming: killing segmenter");
                    segmenterApplication.Kill();
                }
            }
            catch (Exception ex)
            {
                Log.Error("HTTPLiveStreaming: failed to kill segmenter", ex);
            }

            // this really, really isn't the best way to do it but it's the easiest
            new Thread(new ThreadStart(delegate()
            {
                try
                {
                    Thread.Sleep(5000);
                    Directory.Delete(TemporaryDirectory, true);
                }
                catch (Exception ex)
                {
                    Log.Warn("HTTPLiveStreaming: failed to delete temporary directory", ex);
                }
            })).Start();

            return true;
        }
    }
}
