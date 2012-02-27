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
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal abstract class HTTPLiveTranscoderWrapper : ITranscoder, ICustomActionTranscoder
    {
        public string Identifier
        {
            get { return obj.Identifier; }
            set { obj.Identifier = value; }
        }

        private StreamContext context;
        private ITranscoder obj;
        private HTTPLiveStreamingUnit segmenterUnit;

        protected HTTPLiveTranscoderWrapper(ITranscoder toWrap)
        {
            obj = toWrap;
        }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/CustomTranscoderData?parameters=&action=playlist&identifier=" + Identifier;
        }

        public void BuildPipeline(StreamContext context)
        {
            this.context = context;
            obj.BuildPipeline(context);

            segmenterUnit = new HTTPLiveStreamingUnit(Identifier);
            segmenterUnit.DebugOutput = false; // change for debugging

            context.Pipeline.AddDataUnit(segmenterUnit, 20);
        }

        public Stream DoAction(string action, string param)
        {
            switch (action)
            {
                case "segment":
                    WCFUtil.SetContentType(context.Profile.MIME);
                    string segmentPath = Path.Combine(segmenterUnit.TemporaryDirectory, Path.GetFileName(param));
                    if (!File.Exists(segmentPath))
                    {
                        Log.Warn("Requested non-existing segment file {0}", segmentPath);
                        return null;
                    }
                    return new FileStream(segmentPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                case "playlist":
                    WCFUtil.SetContentType("application/vnd.apple.mpegurl");
                    string playlistPath = Path.Combine(segmenterUnit.TemporaryDirectory, "playlist.m3u8");
                    if (!File.Exists(playlistPath))
                    {
                        // playlist not yet created. technically StartStream shouldn't have returned now but that's not implemented yet. 
                        // client should retry after 5s delay
                        return null;
                    }
                    return new FileStream(playlistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                default:
                    Log.Warn("Requested invalid action for HTTP live streaming: {0}", action);
                    return null;
            }
        }
    }
}
