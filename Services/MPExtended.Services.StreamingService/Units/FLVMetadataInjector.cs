#region Copyright (C) 2011-2013 MPExtended
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
using System.Threading.Tasks;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Units
{
    internal class FLVMetadataInjector : IProcessingUnit
    {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private StreamContext context;
        private Task injectionTask;
        private NamedPipe pipeClient;
        private NamedPipe pipeServer;

        public FLVMetadataInjector(StreamContext context)
        {
            this.context = context;
        }

        public bool Setup()
        {
            // Setup the named pipes
            string pipeName = Guid.NewGuid().ToString();
            pipeServer = new NamedPipe(pipeName);
            pipeClient = new NamedPipe(pipeName);
            pipeServer.Start(false);
            pipeClient.Start(true);

            return true;
        }

        public bool Start()
        {
            DataOutputStream = pipeClient;

            injectionTask = Task.Factory.StartNew(delegate()
            {
                byte[] bytes = new byte[1000];
                int result = InputStream.Read(bytes, 0, 1000);
                string bytesToFile = ByteArrayToString(bytes);
                string onMetaData = bytesToFile.Substring(27, 10);
                // if "onMetaData" exists then proceed to read the attributes
                if (onMetaData == "onMetaData")
                {
                    int indexDuration = bytesToFile.IndexOf("duration");
                    double durationOld = GetNextDouble(bytes, indexDuration + 9, 8);
                    double durationNew = ((double)this.context.MediaInfo.Duration) / 1000;
                    byte[] newDur = DoubleToByteArray(durationNew, true);

                    for (int i = 0; i < 8; i++)
                    {
                        bytes[indexDuration + 9 + i] = newDur[i];
                    }
                }
                String bytesToFileNew = ByteArrayToString(bytes);
                pipeServer.Write(bytes, 0, result);

                StreamCopy.AsyncStreamCopy(InputStream, pipeServer);
            }, TaskCreationOptions.LongRunning);
            injectionTask.LogOnException();
            return true;
        }

        public bool Stop()
        {
            // do not abort the injection here - it automatically stops as it's input stream ends, and it only makes things more complicated in the implementation
            return true;
        }

        private Double GetNextDouble(Byte[] b, int offset, int length)
        {
            MemoryStream ms = new MemoryStream(b);
            // move the desired number of places in the array
            ms.Seek(offset, SeekOrigin.Current);
            // create byte array
            byte[] bytes = new byte[length];
            // read bytes
            int result = ms.Read(bytes, 0, length);
            // convert to double (all flass values are written in reverse order)
            return ByteArrayToDouble(bytes, true);
        }

        private string ByteArrayToString(byte[] bytes)
        {
            string byteString = string.Empty;
            foreach (byte b in bytes)
            {
                byteString += Convert.ToChar(b).ToString();
            }
            return byteString;
        }

        private Double ByteArrayToDouble(byte[] bytes, bool readInReverse)
        {
            if (bytes.Length != 8)
                throw new Exception("bytes must be exactly 8 in Length");
            if (readInReverse)
                Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        private byte[] DoubleToByteArray(Double _value, bool writeInReverse)
        {
            byte[] bytes = BitConverter.GetBytes(_value);

            if (writeInReverse)
                Array.Reverse(bytes);

            return bytes;
        }
    }
}
