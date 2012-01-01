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
using System.Threading;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLCWrapperRTSP : VLCWrapper
    {
        private string address;

        public VLCWrapperRTSP()
        {
            readOutputStream = false;
            int nr = new Random().Next(10000, 99999);
            address = "rtsp://{0}:5544/" + nr + ".sdp";
        }

        protected override Tuple<string, string> GetEncoderMuxerParameters(StreamContext context)
        {
            return new Tuple<string, string>(
                context.Profile.CodecParameters["encoder"],
                context.Profile.CodecParameters["muxer"].Replace("#ADDRESS#", String.Format(address, ""))
            );
        }

        public override string GetStreamURL()
        {
            // wait a few seconds till stream is ready
            Thread.Sleep(2500);

            return String.Format(address, WCFUtil.GetCurrentHostname());
        }
    }
}
