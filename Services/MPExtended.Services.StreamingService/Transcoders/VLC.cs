#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLC : VLCBaseTranscoder
    {
        protected bool readOutputStream = true;

        protected override void AddEncoderToPipeline(bool hasInputReader)
        {
            // VLC doesn't support output parsing, but subclasses do
            AddEncoderToPipeline(hasInputReader, EncoderUnit.LogStream.None);
        }

        protected virtual void AddEncoderToPipeline(bool hasInputReader, EncoderUnit.LogStream log)
        {
            // get parameters
            VLCParameters vlcparam = GenerateVLCParameters();
            string path = @"\#OUT#";
            string sout = vlcparam.Sout.Replace("#OUT#", path);

            // generate vlc argument string
            var quotedArgList = vlcparam.Arguments.Select(x => x.Replace("\"", "\\\""));
            string vlcArguments = "\"" + String.Join("\" \"", quotedArgList) + "\"";
            string arguments = GenerateArguments(vlcparam.Input, sout, vlcArguments);

            // add the unit
            //bool readOutputStream = context
            EncoderUnit.TransportMethod input = hasInputReader ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit.TransportMethod outputMethod = readOutputStream ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            // waiting for output pipe is meaningless for VLC as it opens it way earlier then that it actually writes to it. Instead, log parsing
            // in VLCWrapped handles the delay (yes, this class is standalone probably useless but is provided for debugging).
            EncoderUnit unit = new EncoderUnit(Context.Profile.CodecParameters["path"], arguments, input, outputMethod, log);
            unit.DebugOutput = false; // change this for debugging
            Context.Pipeline.AddDataUnit(unit, 5);
        }

        protected virtual string GenerateArguments(string input, string sout, string args)
        {
            return String.Format("-I dummy -vvv \"{0}\" {1} --sout \"{2}\"", input, args, sout);
        }
    }
}
