#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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

namespace MPExtended.Services.StreamingService.Units
{
    internal class HTTPLiveStreamUnit : IProcessingUnit
    {
        public Stream InputStream { get; set; }
        public Stream DataOutputStream { get; private set; }
        public Stream LogOutputStream { get; private set; }
        public bool IsInputStreamConnected { get; set; }
        public bool IsDataStreamConnected { get; set; }
        public bool IsLogStreamConnected { get; set; }

        private string indexFile;

        public HTTPLiveStreamUnit(string indexFile)
        {
            this.indexFile = indexFile;
        }

        public bool Setup()
        {
            DataOutputStream = InputStream;
            return true;
        }

        public bool Start()
        {
            // wait till the index file has been written
            while (!File.Exists(indexFile))
                System.Threading.Thread.Sleep(100);

            return true;
        }

        public bool Stop()
        {
            // cleanup the old files after a while
            Task.Factory.StartNew(delegate()
            {
                try
                {
                    Thread.Sleep(5000);
                    var dir = Path.GetDirectoryName(indexFile);
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to delete HTTP Live Streaming directory. Either your PC is horribly slow or a 0.001% chance of collision happened.", ex);
                }
            });

            return true;
        }
    }
}
