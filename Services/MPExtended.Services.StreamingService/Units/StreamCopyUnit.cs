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
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Units {
    internal class StreamCopyUnit : IProcessingUnit {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private Stream outputStream;
        private string logIdentifier = null;

        public StreamCopyUnit(Stream outputStream) {
            this.outputStream = outputStream;
        }

        public StreamCopyUnit(Stream outputStream, string logIdentifier) :
            this(outputStream) {
            this.logIdentifier = logIdentifier;
        }

        public bool Setup() {
            this.DataOutputStream = this.outputStream;
            return true;
        }

        public bool Start() {
            StreamCopy.AsyncStreamCopy(InputStream, DataOutputStream, logIdentifier);
            return true;
        }

        public bool Stop() {
            return true;
        }
    }
}
