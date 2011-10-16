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
using System.Threading;
using MPExtended.Libraries.VLCManaged;
using MPExtended.Libraries.General;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Units
{
    internal class VLCManagedEncoder : IProcessingUnit
    {
        private const int POLL_DATA_TIME = 1000;

        public enum InputMethod
        {
            File,
            NamedPipe
        }

        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private string sout;
        private string[] arguments;
        private InputMethod inputMethod;
        private string inputPath;
        private int startPos; // in seconds
        private int duration; // in seconds
        private VLCTranscoder transcoder;
        private Reference<WebTranscodingInfo> info;
        private Thread infoReader;

        public VLCManagedEncoder(string sout, string[] arguments, int startPos, int duration, Reference<WebTranscodingInfo> info, InputMethod inputMethod)
        {
            this.sout = sout;
            this.arguments = arguments;
            this.info = info;
            this.inputMethod = inputMethod;
            this.startPos = startPos;
            this.duration = duration;
        }

        public VLCManagedEncoder(string sout, string[] arguments, int startPos, int duration, Reference<WebTranscodingInfo> info, InputMethod inputMethod, string input)
            : this(sout, arguments, startPos, duration, info, inputMethod)
        {
            this.inputPath = input;
        }

        public bool Setup()
        {
            // setup output named pipe
            DataOutputStream = new NamedPipe();
            string output = ((NamedPipe)DataOutputStream).Url;
            Log.Info("VLCManagedEncoder: starting output named pipe {0}", output);
            ((NamedPipe)DataOutputStream).Start(false);

            // prepare sout, needs some trickery for vlc
            string realSout = sout.Replace("#OUT#", @"\" + output);
            
            // debug
            Log.Debug("VLCManagedEncoder: sout {0}", realSout);
            Log.Debug("VLCManagedEncoder: arguments {0}", String.Join("|", arguments));

            // start vlc
            transcoder = new VLCTranscoder();
            transcoder.SetArguments(arguments);
            transcoder.SetMediaName(Guid.NewGuid().ToString());
            transcoder.SetSout(realSout);

            // generate url of input
            if (inputMethod == InputMethod.NamedPipe) 
            {
                inputPath = ((NamedPipe)InputStream).Url;
            }
            Log.Debug("VLCManagedEncoder: input {0}", inputPath);
            transcoder.SetInput(inputPath);

            // start transcoding
            transcoder.StartTranscoding();
            // doesn't work
            //transcoder.Seek(startPos * 1.0 / duration);
            info.Value.Supported = true;

            return true;
        }

        public bool Start()
        {
            // delay start of next unit till our output stream is ready
            Log.Info("VLCManagedEncoder: Waiting till output named pipe is ready");
            ((NamedPipe)DataOutputStream).WaitTillReady();

            // TODO: wait for state machine

            // setup data thread
            infoReader = new Thread(InfoThread);
            infoReader.Name = "VLCInfoThread";
            infoReader.Start(startPos);

            return true;
        }

        public bool Stop()
        {
            Log.Debug("VLCManagedEncoder: Stopping transcoding");
            try
            {
                DataOutputStream.Close();
            }
            catch (Exception e)
            {
                Log.Info("VLCManagedEncoder: Failed to close data output stream", e);
            }

            infoReader.Abort();
            Log.Trace("VLCManagedEncoder: Trying to stop vlc");
            transcoder.StopTranscoding();
            transcoder = null;
            Log.Debug("VLCManagedEncoder: Stopped transcoding");

            return true;
        }

        private void InfoThread(object passedPosition)
        {
            TranscodingInfoCalculator calculator = new TranscodingInfoCalculator((int)passedPosition * 1000, 25, POLL_DATA_TIME);
            while (true)
            {
                try
                {
                    calculator.NewData(transcoder.GetTime() / 1000); // vlc gives time in microseconds
                    calculator.SetStats(info);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to get VLC data", ex);
                }

                Thread.Sleep(POLL_DATA_TIME);
            }
        }
    }
}