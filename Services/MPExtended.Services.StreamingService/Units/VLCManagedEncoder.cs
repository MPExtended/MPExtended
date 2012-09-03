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
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using MPExtended.Libraries.VLCManaged;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
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
        private StreamContext context;
        private InputMethod inputMethod;
        private string inputPath = null;
        private VLCTranscoder transcoder; // VLCNative object, not ITranscoder

        private Timer inputTimer;
        private Reference<WebTranscodingInfo> infoReference;
        private TranscodingInfoCalculator calculator;

        private NamedPipe transcoderInputStream;

        public VLCManagedEncoder(string sout, string[] arguments, StreamContext context, InputMethod inputMethod)
        {
            this.sout = sout;
            this.arguments = arguments;
            this.inputMethod = inputMethod;
            this.context = context;
        }

        public VLCManagedEncoder(string sout, string[] arguments, StreamContext context, InputMethod inputMethod, string input)
            : this(sout, arguments, context, inputMethod)
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

            // setup input
            if (inputMethod == InputMethod.NamedPipe)
            {
                transcoderInputStream = new NamedPipe();
                Log.Info("VLCManagedEncoder: starting input named pipe {0}", transcoderInputStream);
                ((NamedPipe)transcoderInputStream).Start(false);
                inputPath = "stream://" + transcoderInputStream.Url; // use special syntax for VLC to pick up named pipe
            }
            Log.Debug("VLCManagedEncoder: input {0}", inputPath);
            transcoder.SetInput(inputPath);

            // start transcoding
            transcoder.StartTranscoding();
            // doesn't work
            //transcoder.Seek(startPos * 1.0 / context.MediaInfo.Duration * 1000);

            context.TranscodingInfo.Supported = true;
            return true;
        }

        public bool Start()
        {
            // setup input
            if (inputMethod == InputMethod.NamedPipe)
            {
                ((NamedPipe)transcoderInputStream).WaitTillReady();
                Log.Info("VLCManagedEncoder: Copy stream of type {0} into transcoder input pipe", InputStream.ToString());
                StreamCopy.AsyncStreamCopy(InputStream, transcoderInputStream, "transinput");
            }

            // delay start of next unit till our output stream is ready
            Log.Info("VLCManagedEncoder: Waiting till output named pipe is ready");
            ((NamedPipe)DataOutputStream).WaitTillReady();

            // TODO: wait for state machine

            // setup the data reading
            infoReference = new Reference<WebTranscodingInfo>(() => context.TranscodingInfo, x => { context.TranscodingInfo = x; });
            if (context.MediaInfo.Duration > 0)
            {
                Log.Trace("VLCManagedInfo: duration known; is {0}", context.MediaInfo.Duration);
                calculator = new TranscodingInfoCalculator(context.StartPosition, 25, POLL_DATA_TIME, context.MediaInfo.Duration);
            }
            else
            {
                Log.Trace("VLCManagedInfo: duration unknown");
                calculator = new TranscodingInfoCalculator(context.StartPosition, 25, POLL_DATA_TIME);
            }

            // and setup the timer
            inputTimer = new Timer()
            {
                AutoReset = true,
                Interval = POLL_DATA_TIME
            };
            inputTimer.Elapsed += InfoTimerTick;
            inputTimer.Start();
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

            inputTimer.Enabled = false;
            Log.Trace("VLCManagedEncoder: Trying to stop vlc");
            transcoder.StopTranscoding();
            transcoder = null;
            Log.Debug("VLCManagedEncoder: Stopped transcoding");

            return true;
        }

        private void InfoTimerTick(object source, ElapsedEventArgs args)
        {
            while (true)
            {
                try
                {
                    // let's ignore the time here, for reasons detailed in VLCWrapperParsingUnit.cs around line 115
                    float position = transcoder.GetPosition();
                    Log.Trace("VLCManagedInfo: calling NewPercentage with position {0}", position);
                    calculator.NewPercentage(position);
                    calculator.SaveStats(infoReference);
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to get VLC data", ex);
                }
            }
        }
    }
}