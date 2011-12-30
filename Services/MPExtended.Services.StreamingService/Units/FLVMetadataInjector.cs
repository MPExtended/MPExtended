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
using System.Threading;
using MPExtended.Libraries.Service;
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
        private Thread doInjectionThread;
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
            doInjectionThread = ThreadManager.Start("FLVMetadataInjection", delegate()
            {
                DataOutputStream = pipeClient;

                // Do the actual metadata injection here:
                // - you can read the VLC output from InputStream
                // - write your output to pipeServer
                MPExtended.Services.StreamingService.Code.StreamCopy.AsyncStreamCopy(InputStream, pipeServer);
            });
            return true;
        }

        public bool Stop()
        {
            ThreadManager.Abort(doInjectionThread);
            return true;
        }
    }
}
