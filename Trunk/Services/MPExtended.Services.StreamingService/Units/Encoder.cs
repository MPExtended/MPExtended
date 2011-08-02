#region Copyright
/* 
 *  Copyright (C) 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Units {
    internal class EncoderUnit : IProcessingUnit {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        public string Source { get; set; }
        public string Output { get; set; }
        public bool DebugOutput { get; set; }

        private string transcoderPath;
        private string arguments;
        private TransportMethod inputMethod;
        private TransportMethod outputMethod;
        private Process transcoderApplication;
        private Stream transcoderInputStream;
        private bool doInputCopy;

        public EncoderUnit(string transcoder, string arguments, TransportMethod inputMethod, TransportMethod outputMethod) {
            this.transcoderPath = transcoder;
            this.arguments = arguments;
            this.inputMethod = inputMethod;
            this.outputMethod = outputMethod;
        }

        public bool Setup() {
            // sets up streams
            string input = "";
            string output = "";
            bool needsStdin = false;
            bool needsStdout = false;

            // input (StandardOut not supported and External needs no processing)
            if (inputMethod == TransportMethod.Filename) {
                input = this.Source;
            } else if (inputMethod == TransportMethod.NamedPipe) {
                transcoderInputStream = new NamedPipe();
                input = ((NamedPipe)transcoderInputStream).Url;
                Log.Info("Encoding: starting input named pipe {0}", input);
                ((NamedPipe)transcoderInputStream).Start(false);
                doInputCopy = true;
            } else if (inputMethod == TransportMethod.StandardIn) {
                needsStdin = true;
                doInputCopy = true;
            } 

            // output stream (StandardIn not supported and External needs no processing)
            if (outputMethod == TransportMethod.Filename) {
                if(this.Output == null)
                    this.Output = Path.GetTempFileName();
                output = this.Output;
            } else if (outputMethod == TransportMethod.NamedPipe) {
                DataOutputStream = new NamedPipe();
                output = ((NamedPipe)DataOutputStream).Url;
                Log.Info("Encoding: starting output named pipe {0}", output);
                ((NamedPipe)DataOutputStream).Start(false);
            } else if (outputMethod == TransportMethod.StandardOut) {
                needsStdout = true;
            }

            // start transcoder
            Log.Info("Encoding: Transcoder configuration dump");
            Log.Info("Encoding:   input {0}, output {1}", input, output);
            Log.Info("Encoding:   needsStdin {0}, needsStdout {1}", needsStdin, needsStdout);
            Log.Info("Encoding:   path {0}", transcoderPath);
            Log.Info("Encoding:   arguments {0}", arguments);
            if (!SpawnTranscoder(input, output, needsStdin, needsStdout))
                return false;

            // finish stream setup
            if (inputMethod == TransportMethod.StandardIn)
                transcoderInputStream = transcoderApplication.StandardInput.BaseStream;
            if (outputMethod == TransportMethod.StandardOut)
                DataOutputStream = transcoderApplication.StandardOutput.BaseStream;
            if (outputMethod == TransportMethod.Filename)
                DataOutputStream = new FileStream(output, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // doesn't work yet

            // setup stderr forwarding, if not debugging output
            if (IsLogStreamConnected && !DebugOutput)
            {
                LogOutputStream = transcoderApplication.StandardError.BaseStream;
            }
            else if (IsLogStreamConnected && DebugOutput)
            {
                LogOutputStream = new MemoryStream(4096);
            }

            return true;
        }

        private bool SpawnTranscoder(string input, string output, bool needsStdin, bool needsStdout) {
            string args = arguments.Replace("#IN#", input).Replace("#OUT#", output);
            ProcessStartInfo start = new ProcessStartInfo(transcoderPath, args);
            start.UseShellExecute = DebugOutput && !IsLogStreamConnected && !needsStdin && !needsStdout;
            start.RedirectStandardInput = needsStdin;
            start.RedirectStandardOutput = needsStdout;
            start.RedirectStandardError = DebugOutput ? false : IsLogStreamConnected;
            start.WindowStyle = DebugOutput ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            start.CreateNoWindow = !DebugOutput;

            try {
                transcoderApplication = new Process();
                transcoderApplication.StartInfo = start;
                transcoderApplication.Start();
            } catch (Win32Exception e) {
                Log.Error("Encoding: Failed to start transcoder", e);
                Log.Info("ERROR: Transcoder probably doesn't exists");
                return false;
            }
            return true;
        }

        public bool Start() {
            // wait for the input pipe to be ready
            if (transcoderInputStream is NamedPipe)
                ((NamedPipe)transcoderInputStream).WaitTillReady();

            // copy the inputStream to the transcoderInputStream
            if(doInputCopy) {
                Log.Info("Encoding: Copy stream of type {0} into transcoder input stream of type {1}", InputStream.ToString(), transcoderInputStream.ToString());
                StreamCopy.AsyncStreamCopy(InputStream, transcoderInputStream, "transinput");
            }

            // delay start of next unit till our output stream is ready
            if (DataOutputStream is NamedPipe) {
                Log.Info("Encoding: Waiting till output named pipe is ready");
                ((NamedPipe)DataOutputStream).WaitTillReady();
            }

            return true;
        }

        public bool Stop() {
            // close streams
            CloseStream(InputStream, "input");
            CloseStream(transcoderInputStream, "transcoder input");
            CloseStream(DataOutputStream, "transcoder output");

            try  {
                if (transcoderApplication != null && !transcoderApplication.HasExited) {
                    Log.Debug("Encoding: Killing transcoder");
                    transcoderApplication.Kill();
                }
            } catch (Exception e) {
                Log.Error("Encoding: Failed to kill transcoder", e);
            }

            return true;
        }

        private void CloseStream(Stream stream, string logName) {
            try {
                if (stream != null) stream.Close();
            } catch (Exception e) {
                Log.Info("Encoding: Failed to close {0} stream: {1}", logName, e.Message);
            }
        }   
    }
}
