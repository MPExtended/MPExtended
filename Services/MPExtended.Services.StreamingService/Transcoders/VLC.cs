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
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLC : VLCBaseTranscoder
    {
        protected bool readOutputStream = true;

        public override void BuildPipeline(StreamContext context)
        {
            // VLC doesn't support output parsing, but subclasses do
            BuildPipeline(context, EncoderUnit.LogStream.None);
        }

        public void BuildPipeline(StreamContext context, EncoderUnit.LogStream log)
        {            
            // input
            bool doInputReader = context.Source.DoesNeedInputReader();
            if(doInputReader)
            {
                context.Pipeline.AddDataUnit(context.Source.GetInputReaderUnit(), 1);
            }

            // get parameters
            VLCParameters vlcparam = GenerateVLCParameters(context);

            // prepare vlc arguments
            string path = @"\#OUT#";
            string sout = vlcparam.Sout.Replace("#OUT#", path);
            string arguments = GenerateArguments(vlcparam.Input, sout, String.Join(" ", vlcparam.Arguments));

            // add the unit
            EncoderUnit.TransportMethod input = doInputReader ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit.TransportMethod outputMethod = readOutputStream ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            // waiting for output pipe is meaningless for VLC as it opens it way earlier then that it actually writes to it. Instead, log parsing
            // in VLCWrapped handles the delay (yes, this class is standalone probably useless but is provided for debugging).
            EncoderUnit unit = new EncoderUnit(context.Profile.CodecParameters["path"], arguments, input, outputMethod, log);
            unit.DebugOutput = false; // change this for debugging
            context.Pipeline.AddDataUnit(unit, 5);
        }

        protected virtual string GenerateArguments(string input, string sout, string args)
        {
            return String.Format("-I dummy -vvv \"{0}\" {1} --sout \"{2}\"", input, args, sout);
        }
    }
}
