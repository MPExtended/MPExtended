#region Copyright
// Copyright (C) 2008, 2009 StreamTv, http://code.google.com/p/mpstreamtv/
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MPExtended.Libraries.Service;

namespace MPExtended.Services.StreamingService.Code
{
    internal class StreamCopy
    {
        private const int DEFAULT_BUFFER_SIZE = 0x10000;

        private byte[] buffer;
        private int bufferSize;
        private Stream source;
        private Stream destination;
        private string log;

        private StreamCopy(Stream source, Stream destination, int bufferSize, string log)
        {
            this.source = source;
            this.destination = destination;
            this.bufferSize = bufferSize;
            this.log = log;
        }

        private void StartCopy(bool retry)
        {
            buffer = new byte[bufferSize];
            try
            {
                source.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, new object());
            }
            catch (NotSupportedException e)
            {
                // we only diagnose problems with TsBuffer, other streams are supposed to work correctly
                if (!(source is TsBuffer))
                    throw;

                // TODO: is this still needed or are the TsBuffer problems solved?
                TsBuffer stream = (TsBuffer)source;
                Log.Error(string.Format("StreamCopy {0}: NotSupportedException when trying to read from TsBuffer", log), e);
                Log.Debug("StreamCopy {0}: TsBuffer dump: CanRead {1}, CanWrite {2}", log, stream.CanRead, stream.CanWrite);
                Log.Debug("StreamCopy {0}:\r\n{1}", log, stream.DumpStatus());
                if (retry)
                {
                    Thread.Sleep(500);
                    Log.Info("StreamCopy {0}: Trying to recover", log);
                    StartCopy(false);
                }
            }
        }

        private void CopyStream()
        {
            StartCopy(true);
        }

        private void MediaReadAsyncCallback(IAsyncResult ar)
        {
            try
            {
                int read = source.EndRead(ar);
                if (read == 0) // empty result indicates end-of-stream
                    return;

                // write read bytes to the destination
                destination.BeginWrite(buffer, 0, read, writeResult =>
                {
                    try
                    {
                        destination.EndWrite(writeResult);
                        destination.Flush();

                        // and read again...
                        source.BeginRead(buffer, 0, buffer.Length, MediaReadAsyncCallback, new object());
                    }
                    catch (Exception e)
                    {
                        HandleException(e, "inner");
                    }
                }, null);
            }
            catch (Exception e)
            {
                HandleException(e, "outer");
            }
        }


        private void HandleException(Exception e, string type)
        {
            if (e is IOException)
            {
                // usually end-of-pipe error
                Log.Info("StreamCopy {0}: IOException in {1} stream copy, is usually ok: {2}", log, type, e.Message);
            }
            else
            {
                Log.Error(String.Format("StreamCopy {0}: Failure in {1} stream copy", log, type), e);
            }
        }

        public static void AsyncStreamCopy(Stream source, Stream destination, string logIdentifier, int bufferSize)
        {
            StreamCopy copy = new StreamCopy(source, destination, bufferSize, logIdentifier);
            copy.CopyStream();
        }

        public static void AsyncStreamCopy(Stream source, Stream destination, string logIdentifier)
        {
            AsyncStreamCopy(source, destination, logIdentifier, DEFAULT_BUFFER_SIZE);
        }

        public static void AsyncStreamCopy(Stream source, Stream destination)
        {
            AsyncStreamCopy(source, destination, "", DEFAULT_BUFFER_SIZE);
        }


        public static void AsyncStreamRead(StreamReader input, Action<string> lineHandler)
        {
            Task.Factory.StartNew(delegate()
            {
                string line;
                while ((line = input.ReadLine()) != null)
                    lineHandler.Invoke(line);
            });
        }
    }
}