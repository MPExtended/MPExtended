#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Units
{
    internal class InputUnit : IProcessingUnit
    {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private string identifier;
        private string source;

        public InputUnit(string identifier, string source)
        {
            this.identifier = identifier;
            this.source = source;
        }

        public bool Setup()
        {
            try
            {
                if (source.IndexOf(".ts.tsbuffer") != -1)
                {
                    StreamLog.Info(identifier, "Using TsBuffer to read input");
                    DataOutputStream = new TsBuffer(this.source);
                }
                else
                {
                    StreamLog.Info(identifier, "Using FileStream to read input");
                    var baseStram = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, short.MaxValue, FileOptions.RandomAccess);
                    DataOutputStream = new DirectStream(baseStram, identifier);
                }
            }
            catch (Exception e)
            {
                StreamLog.Error(identifier, "Failed to setup InputProcessingUnit", e);
                return false;
            }
            return true;
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            var directStream = DataOutputStream as DirectStream;
            if (directStream != null)
            {
                directStream.Stop();
            }
            else
            {
                DataOutputStream.Close();
            }
            return true;
        }

        #region Helper class DirectStream

        /// <summary>
        /// A Wraper for Ignore Close from WCF while Seeking
        /// </summary>
        public class DirectStream : Stream
        {
            Stream baseStream;
            string identifier;

            public DirectStream(Stream baseStream, string identifier)
            {
                this.baseStream = baseStream;
                this.identifier = identifier;
            }

            public override bool CanRead { get {return baseStream.CanRead; } }

            public override bool CanSeek { get {return baseStream.CanSeek;} }

            public override bool CanWrite { get { return false; } }

            public override long Length { get { return baseStream.Length; } }

            public override long Position
            {
                get { return baseStream.Position; }
                set { baseStream.Position = value; }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return baseStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return baseStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                baseStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Close is ignored! Use <see cref="Stop"/> to Close the stream
            /// </summary>
            public override void Close()
            {
                if (CanSeek)
                {
                    StreamLog.Debug(identifier, "DirectStream Close ignored");
                }
            }

            public void Stop()
            {
                base.Close();
                StreamLog.Debug(identifier, "DirectStream Close");
            }
        }

        #endregion

    }
}
