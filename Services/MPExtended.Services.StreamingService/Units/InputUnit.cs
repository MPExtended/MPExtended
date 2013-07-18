﻿#region Copyright (C) 2011-2013 MPExtended
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
                    DataOutputStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
            DataOutputStream.Close();
            return true;
        }
    }
}
