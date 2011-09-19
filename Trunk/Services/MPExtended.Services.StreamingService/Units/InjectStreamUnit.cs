#region Copyright (C) 2011 MPExtended
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
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Units {
    internal class InjectStreamUnit : IProcessingUnit {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private Stream source;

        public InjectStreamUnit(Stream source) {
            this.source = source;
        }

        public bool Setup() {
            this.DataOutputStream = this.source;
            return true;
        }

        public bool Start() {
            return true;
        }

        public bool Stop() {
            this.source.Close();
            return true;
        }
    }
}
