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
using System.IO;
using MPExtended.Services.StreamingService.Util;

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
