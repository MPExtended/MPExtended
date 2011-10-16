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
using System.Linq;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLCManaged : VLCBaseTranscoder
    {
        protected bool readOutputStream = true;

        public override string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public override void AlterPipeline(Pipeline pipeline, WebResolution outputSize, Reference<WebTranscodingInfo> einfo, int position, int? audioId, int? subtitleId)
        {
            SetupAssemblyLoader();

            // input
            bool doInputReader = !Source.IsLocalFile;
            if (doInputReader)
            {
                pipeline.AddDataUnit(Source.GetInputReaderUnit(), 1);
            }

            // get parameters
            VLCParameters vlcparam = GenerateVLCParameters(outputSize, position, audioId, subtitleId);
            int duration = (int)Math.Round((decimal)MediaInfo.Duration / 1000);

            // add the unit
            VLCManagedEncoder unit;
            if (doInputReader)
            {
                unit = new VLCManagedEncoder(vlcparam.Sout, vlcparam.Arguments, position, duration, einfo, VLCManagedEncoder.InputMethod.NamedPipe);
            }
            else
            {
                unit = new VLCManagedEncoder(vlcparam.Sout, vlcparam.Arguments, position, duration, einfo, VLCManagedEncoder.InputMethod.File, Source.GetPath());
            }
            pipeline.AddDataUnit(unit, 5);
        }

        private void SetupAssemblyLoader()
        {
            // MPExtended.Libraries.VLCManaged.dll is in the vlc-1.1.xx directory. We should load it from there so that it can properly find libvlc
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(delegate(object sender, ResolveEventArgs args) 
            {
                if (args.Name.StartsWith("MPExtended.Libraries.VLCManaged"))
                {
#if DEBUG
                    string directory = System.IO.Path.Combine(Installation.GetRootDirectory(), "Libraries", "Streaming", "vlc-1.1.11");
#else
                    string directory = System.IO.Path.Combine(Installation.GetRootDirectory(), "vlc-1.1.11");
#endif

                    string asmPath = System.IO.Path.Combine(directory, "MPExtended.Libraries.VLCManaged.dll");
                    return System.Reflection.Assembly.LoadFrom(asmPath);
                }
                return null;
            });
        }
    }
}
