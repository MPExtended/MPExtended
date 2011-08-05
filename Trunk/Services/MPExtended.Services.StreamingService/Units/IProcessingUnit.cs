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

namespace MPExtended.Services.StreamingService.Units {
    internal interface IProcessingUnit {
        Stream InputStream { get; set; }
        Stream DataOutputStream { get; }
        Stream LogOutputStream { get; }
        bool IsInputStreamConnected { get; set; }
        bool IsDataStreamConnected { get; set; }
        bool IsLogStreamConnected { get; set; }

        bool Setup();
        bool Start();
        bool Stop();
    }
}
