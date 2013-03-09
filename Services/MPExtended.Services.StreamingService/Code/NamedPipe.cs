#region Copyright
// Copyright (C) 2008, 2009 StreamTv, http://code.google.com/p/mpstreamtv/
// Copyright (C) 2009, 2010 Gemx
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
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.Services.StreamingService.Code {
    internal class NamedPipe : Stream {
        private String _pipeName;
        private Boolean isReady;

        private PipeStream pipe;

        public String Url {
            get { return String.Format(@"\\.\pipe\{0}", _pipeName); }
        }

        public bool IsReady {
            get { return isReady; }
        }

        public override bool CanRead {
            get { return pipe != null && pipe.CanRead; }
        }

        public override bool CanSeek {
            get { return pipe != null && pipe.CanSeek; }
        }

        public override bool CanWrite {
            get { return pipe != null && pipe.CanWrite; }
        }

        public override long Length {
            get { return pipe.Length; }
        }

        public override long Position {
            get {
                return pipe.Position;
            }
            set {
                pipe.Position = value;
            }
        }

        public NamedPipe() {
            _pipeName = Guid.NewGuid().ToString();
        }

        public NamedPipe(String pipeName) {
            _pipeName = pipeName;
        }

        public override void Flush() {
            pipe.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int read = 0;
            read = pipe.Read(buffer, offset, count);
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return pipe.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            pipe.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            try
            {
                pipe.Write(buffer, offset, count);
            }
            catch (IOException e)
            {
                // Sometimes we are still writing to the input named pipe while we already have killed the transcoder, or the transcoder has already
                // finished and we didn't detect that yet. This is no problem as the stream should stop in both cases, and writing is stopped a few lines
                // of code later.
                Log.Debug("IOException in NamedPipe.Write, is usually no problem", e);
            }
        }

        public void Start(Boolean isClient) {
            if (isClient) {
                NamedPipeClientStream client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                client.Connect(10000); // 10 second timeout.

                pipe = client;
                isReady = true;
            } else {
                // Grant read/write access to everyone, so the pipe can be written to from impersonated processes
                PipeSecurity pipeSecurity = new PipeSecurity();
                pipeSecurity.SetAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow)); 
                NamedPipeServerStream server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 4096, 4096, pipeSecurity);
                server.BeginWaitForConnection(new AsyncCallback(WaitForConnection), server);
            }
        }

        public void WaitTillReady() {
            while (!this.IsReady)
                System.Threading.Thread.Sleep(100);
        }

        private void WaitForConnection(IAsyncResult ar) {
            NamedPipeServerStream server = ar.AsyncState as NamedPipeServerStream;
            server.EndWaitForConnection(ar);

            pipe = server;
            isReady = true;
        }
    }
}
