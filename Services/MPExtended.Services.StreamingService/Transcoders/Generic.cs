#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    class Generic : ITranscoder
    {
        public string Identifier { get; set; }
        public StreamContext Context { get; set; }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public void BuildPipeline()
        {

            // create full argument string
            string program = Context.Profile.TranscoderParameters["transcoder"];
            string arguments = Context.Profile.TranscoderParameters["arguments"]
                .Replace("#WIDTH#", Context.OutputSize.Width.ToString())
                .Replace("#HEIGHT#", Context.OutputSize.Height.ToString())
                .Replace("#AUDIOSTREAMID#", Context.AudioTrackId.ToString())
                .Replace("#STARTPOSITION#", Math.Round((decimal)Context.StartPosition / 1000).ToString());

            // add input reader
            if (Context.Source.NeedsInputReaderUnit)
            {
                Context.Pipeline.AddDataUnit(Context.Source.GetInputReaderUnit(), 1);
            }
            else
            {
                arguments = arguments.Replace("#IN#", Context.Source.GetPath());
            }

            // add unit
            EncoderUnit.TransportMethod input = Context.Source.NeedsInputReaderUnit ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit unit = new EncoderUnit(program, arguments, input, EncoderUnit.TransportMethod.NamedPipe, EncoderUnit.LogStream.None, Context);
            unit.DebugOutput = false; // change this for debugging
            Context.Pipeline.AddDataUnit(unit, 5);

            // setup output parsing
            Context.TranscodingInfo.Supported = false;
        }
    }
}
