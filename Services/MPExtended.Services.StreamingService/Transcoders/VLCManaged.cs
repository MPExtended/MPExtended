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
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLCManaged : VLCBaseTranscoder
    {
        protected bool readOutputStream = true;

        protected override void AddEncoderToPipeline(bool hasInputReader)
        {
            SetupAssemblyLoader();

            // get parameters
            VLCParameters vlcparam = GenerateVLCParameters();
            int duration = (int)Math.Round((decimal)Context.MediaInfo.Duration / 1000);

            // add the unit
            VLCManagedEncoder unit;
            if (hasInputReader)
            {
                unit = new VLCManagedEncoder(vlcparam.Sout, vlcparam.Arguments, Context, VLCManagedEncoder.InputMethod.NamedPipe);
            }
            else
            {
                unit = new VLCManagedEncoder(vlcparam.Sout, vlcparam.Arguments, Context, VLCManagedEncoder.InputMethod.File, Context.Source.GetPath());
            }
            Context.Pipeline.AddDataUnit(unit, 5);
        }

        private void SetupAssemblyLoader()
        {
            // MPExtended.Libraries.VLCManaged.dll is in the vlc-1.1.xx directory. We should load it from there so that it can properly find libvlc
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(delegate(object sender, ResolveEventArgs args) 
            {
                if (args.Name.StartsWith("MPExtended.Libraries.VLCManaged"))
                {
                    string directory = Installation.GetFileLayoutType() == FileLayoutType.Source ?
                        Path.Combine(Installation.GetSourceRootDirectory(), "Libraries", "Streaming", "vlc-1.1.11") :
                        Path.Combine(Installation.GetInstallDirectory(), "Streaming", "vlc-1.1.11");

                    string asmPath = System.IO.Path.Combine(directory, "MPExtended.Libraries.VLCManaged.dll");
                    return System.Reflection.Assembly.LoadFrom(asmPath);
                }
                return null;
            });
        }
    }
}
