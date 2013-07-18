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
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class Direct : ITranscoder, IRetrieveHookTranscoder
    {
        public string Identifier { get; set; }
        public StreamContext Context { get; set; }

        public string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public void BuildPipeline()
        {
            // we ignore our arguments :)
            Context.TranscodingInfo.Supported = false;
            Context.Pipeline.AddDataUnit(Context.GetInputReaderUnit(), 1);
            return;
        }

        public void RetrieveStreamCalled()
        {
            // Don't return a length when we're streaming TV, as we don't know the full length of the stream, since we are
            // reading from a timeshifting buffer. See dicussion in bug #394 and #319. 
            if (Context.Source.MediaType != WebMediaType.TV)
                WCFUtil.SetContentLength(Context.Source.GetFileInfo().Size);

            // there has to be a better way to do this
            object mime = RegistryReader.ReadKey(Microsoft.Win32.RegistryHive.ClassesRoot, Path.GetExtension(Context.Source.GetFileInfo().Name), "Content Type");
            if (mime != null)
            {
                WCFUtil.SetContentType(mime.ToString());
            }
        }
    }
}
