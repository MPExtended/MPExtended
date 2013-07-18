﻿#region Copyright (C) 2011-2013 MPExtended
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Units
{
    internal class EncoderUnit : IProcessingUnit
    {
        public enum TransportMethod
        {
            NamedPipe,
            StandardIn,
            StandardOut,
            Other
        }

        public enum LogStream
        {
            StandardOut,
            StandardError,
            None
        }

        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        public bool DebugOutput { get; set; }

        private string transcoderPath;
        private string arguments;
        private TransportMethod inputMethod;
        private TransportMethod outputMethod;
        private LogStream logStream;
        private TranscoderProcess transcoderApplication;
        private Stream transcoderInputStream;
        private bool doInputCopy;

        private StreamContext context;

        public EncoderUnit(string transcoder, string arguments, TransportMethod inputMethod, TransportMethod outputMethod, LogStream logStream, StreamContext context)
        {
            this.transcoderPath = transcoder;
            this.arguments = arguments;
            this.inputMethod = inputMethod;
            this.outputMethod = outputMethod;
            this.logStream = logStream;
            this.context = context;
        }

        public bool Setup()
        {
            // sets up streams
            string input = "";
            string output = "";
            bool needsStdin = false;
            bool needsStdout = false;

            // input
            if (inputMethod == TransportMethod.NamedPipe)
            {
                transcoderInputStream = new NamedPipe();
                input = ((NamedPipe)transcoderInputStream).Url;
                StreamLog.Info(context.Identifier, "Encoding: starting input named pipe {0}", input);
                ((NamedPipe)transcoderInputStream).Start(false);
                doInputCopy = true;
            }
            else if (inputMethod == TransportMethod.StandardIn)
            {
                needsStdin = true;
                doInputCopy = true;
            }

            // output stream
            if (outputMethod == TransportMethod.NamedPipe)
            {
                DataOutputStream = new NamedPipe();
                output = ((NamedPipe)DataOutputStream).Url;
                StreamLog.Info(context.Identifier, "Encoding: starting output named pipe {0}", output);
                ((NamedPipe)DataOutputStream).Start(false);
            }
            else if (outputMethod == TransportMethod.StandardOut)
            {
                needsStdout = true;
            }

            // arguments substitution
            arguments = arguments.Replace("#IN#", input).Replace("#OUT#", output);

            // start transcoder
            if (!SpawnTranscoder(needsStdin, needsStdout))
                return false;

            // finish stream setup
            if (inputMethod == TransportMethod.StandardIn)
                transcoderInputStream = transcoderApplication.StandardInput.BaseStream;
            if (outputMethod == TransportMethod.StandardOut)
                DataOutputStream = transcoderApplication.StandardOutput.BaseStream;

            // setup log stream
            if (!DebugOutput && logStream == LogStream.StandardOut && IsLogStreamConnected && outputMethod != TransportMethod.StandardOut)
            {
                LogOutputStream = transcoderApplication.StandardOutput.BaseStream;
            }
            else if (!DebugOutput && logStream == LogStream.StandardError && IsLogStreamConnected)
            {
                LogOutputStream = transcoderApplication.StandardError.BaseStream;
            }
            else
            {
                LogOutputStream = new MemoryStream(4096);
            }

            return true;
        }

        private bool SpawnTranscoder(bool needsStdin, bool needsStdout)
        {
            ProcessStartInfo start = new ProcessStartInfo(transcoderPath, arguments);
            start.UseShellExecute = false;
            start.RedirectStandardInput = needsStdin;
            start.RedirectStandardOutput = needsStdout || (!DebugOutput && logStream == LogStream.StandardOut && IsLogStreamConnected);
            start.RedirectStandardError = !DebugOutput && logStream == LogStream.StandardError && IsLogStreamConnected;
            start.WindowStyle = DebugOutput ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            start.CreateNoWindow = !DebugOutput;

            StreamLog.Info(context.Identifier, "Encoder: Transcoder configuration dump");
            StreamLog.Info(context.Identifier, "Encoder:   hasStdin {0}, hasStdout {1}, hasStderr {2}", start.RedirectStandardInput, start.RedirectStandardOutput, start.RedirectStandardError);
            StreamLog.Info(context.Identifier, "Encoder:   path {0}", transcoderPath);
            StreamLog.Info(context.Identifier, "Encoder:   arguments {0}", arguments);

            try
            {
                transcoderApplication = new TranscoderProcess();
                transcoderApplication.StartInfo = start;
                if (context.Source.NeedsImpersonation)
                    transcoderApplication.StartAsUser(Configuration.Services.NetworkImpersonation.Domain, Configuration.Services.NetworkImpersonation.Username, Configuration.Services.NetworkImpersonation.GetPassword());
                else
                    transcoderApplication.Start();
            }
            catch (Win32Exception e)
            {
                StreamLog.Error(context.Identifier, "Encoding: Failed to start transcoder", e);
                return false;
            }
            return true;
        }

        public bool Start()
        {
            // wait for the input pipe to be ready
            if (transcoderInputStream is NamedPipe)
                ((NamedPipe)transcoderInputStream).WaitTillReady();

            // copy the inputStream to the transcoderInputStream
            if (doInputCopy)
            {
                StreamLog.Info(context.Identifier, "Encoding: Copy stream of type {0} into transcoder input stream of type {1}", InputStream.ToString(), transcoderInputStream.ToString());
                StreamCopy.AsyncStreamCopy(InputStream, transcoderInputStream, "transinput");
            }

            // delay start of next unit till our output stream is ready
            if (DataOutputStream is NamedPipe && (outputMethod == TransportMethod.NamedPipe || outputMethod == TransportMethod.StandardOut))
            {
                StreamLog.Trace(context.Identifier, "Transcoder running: {0}", !transcoderApplication.HasExited);
                StreamLog.Info(context.Identifier, "Encoding: Waiting till output named pipe is ready");

                var pipe = (NamedPipe)DataOutputStream;
                var checkFailed = context != null && context.TranscodingInfo != null;
                while (!pipe.IsReady && !(checkFailed && context.TranscodingInfo.Failed))
                    System.Threading.Thread.Sleep(100);

                if (checkFailed && context.TranscodingInfo.Failed)
                {
                    StreamLog.Warn(context.Identifier, "Encoding: Aborting wait because transcoder application failed and will never setup output named pipe.");
                    return false;
                }
            }

            return true;
        }

        public bool Stop()
        {
            // close streams
            CloseStream(InputStream, "input");
            CloseStream(transcoderInputStream, "transcoder input");
            CloseStream(DataOutputStream, "transcoder output");

            try
            {
                if (transcoderApplication != null && !transcoderApplication.HasExited)
                {
                    StreamLog.Debug(context.Identifier, "Encoding: Killing transcoder");
                    transcoderApplication.Stop();
                }
            }
            catch (Exception e)
            {
                StreamLog.Error(context.Identifier, "Encoding: Failed to kill transcoder", e);
            }

            return true;
        }

        private void CloseStream(Stream stream, string logName)
        {
            try
            {
                if (stream != null) stream.Close();
            }
            catch (Exception e)
            {
                StreamLog.Info(context.Identifier, "Encoding: Failed to close {0} stream: {1}", logName, e.Message);
            }
        }
    }
}
