#region Copyright
// Copyright (C) 2008, 2009 StreamTv, http://code.google.com/p/mpstreamtv/
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

using System;
using System.IO;
using System.Threading;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Services.StreamingService.Code {
    internal class StreamCopy {
        private const int _defaultBufferSize = 0x10000;
        private byte[] buffer;
        private Stream source;
        private Stream destination;
        private int bufferSize;
        private string log;

        private StreamCopy(Stream source, Stream destination, int bufferSize, string log) {
            this.source = source;
            this.destination = destination;
            this.bufferSize = bufferSize;
            this.log = log;
        }

        private void CopyStream(bool retry) {
            // do a parallel read
            buffer = new byte[bufferSize];
            try {
                source.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, new object());
            } catch (NotSupportedException e) {
                // we only do a workaround for TsBuffer here, nothing for other errors
                if (!(source is TsBuffer))
                    throw;

                TsBuffer stream = (TsBuffer)source;
                Log.Error(string.Format("StreamCopy {0}: NotSupportedException when trying to read from TsBuffer", log), e);
                Log.Info("StreamCopy {0}: TsBuffer dump: CanRead {1}, CanWrite {2}", log, stream.CanRead, stream.CanWrite);
                Log.Info("StreamCopy {0}:\r\n{1}", log, stream.DumpStatus());
                if (retry) {
                    Thread.Sleep(500);
                    Log.Info("StreamCopy {0}: Trying to recover", log);
                    CopyStream(false);
                }
            }
        }

        private void CopyStream() {
            CopyStream(true);
        }

        private void MediaReadAsyncCallback(IAsyncResult ar) {
            try {
                int read = source.EndRead(ar);
                if (read == 0) // we're done
                    return;

                // write it to the destination
                //Log.Info("StreamCopy {0}: writing {1} bytes", log, read);
                destination.BeginWrite(buffer, 0, read, writeResult => {
                    try {
                        destination.EndWrite(writeResult);
                        destination.Flush();

                        // and read again...
                        source.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, new object());
                    } catch (Exception e) {
                        HandleException(e, "inner");
                    }
                }, null);
            } catch (Exception e) {
                HandleException(e, "outer");
            }
        }


        private void HandleException(Exception e, string type) {
            if (e is IOException) {
                // end of pipe etc
                Log.Info("StreamCopy {0}: IOException in {1} stream copy, is usually ok: {2}", log, type, e.Message);
            } else {
                Log.Error(string.Format("StreamCopy {0}: Failure in {1} stream copy", log, type), e);
            }
        }

        public static void AsyncStreamCopy(Stream original, Stream destination, string logIdentifier, int bufferSize) {
            StreamCopy copy = new StreamCopy(original, destination, bufferSize, logIdentifier);
            copy.CopyStream();
        }

        public static void AsyncStreamCopy(Stream original, Stream destination, string logIdentifier) {
            AsyncStreamCopy(original, destination, logIdentifier, _defaultBufferSize);
        }

        public static void AsyncStreamCopy(Stream original, Stream destination) {
            AsyncStreamCopy(original, destination, "", _defaultBufferSize);
        }
    }
}