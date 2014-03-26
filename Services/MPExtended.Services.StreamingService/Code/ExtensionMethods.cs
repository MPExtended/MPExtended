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
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service.Config;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class ResolutionExtensionMethods
    {
        public static WebResolution ToWebResolution(this Resolution res)
        {
            return new WebResolution()
            {
                Width = res.Width,
                Height = res.Height
            };
        }
    }

    internal static class TranscoderProfileExtensionMethods
    {
        public static WebTranscoderProfile ToWebTranscoderProfile(this TranscoderProfile profile)
        {
            // WCF sucks a bit with returning child classes
            return new WebTranscoderProfile()
            {
                Bandwidth = profile.Bandwidth,
                Description = profile.Description,
                HasVideoStream = profile.HasVideoStream,
                MaxOutputHeight = profile.MaxOutputHeight,
                MaxOutputWidth = profile.MaxOutputWidth,
                MIME = profile.MIME,
                Name = profile.Name,
                Targets = profile.Targets,
                Transport = profile.Transport
            };
        }
    }
}