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
using System.Reflection;

namespace MPExtended.Applications.Development.DevTool.DocGen
{
    internal class WSSGenerator : Generator
    {
        public WSSGenerator(Assembly assembly)
        {
            this.Assembly = assembly;
            this.JsonAPI = this.Assembly.GetType("MPExtended.Services.StreamingService.Interfaces.IWebStreamingService");
            this.StreamAPI = this.Assembly.GetType("MPExtended.Services.StreamingService.Interfaces.IStreamingService");
            this.Enums = new List<Type>() {
                this.Assembly.GetType("MPExtended.Services.StreamingService.Interfaces.WebStreamMediaType"),
                this.Assembly.GetType("MPExtended.Services.StreamingService.Interfaces.WebArtworkType")
            };
        }

        protected override int GenerateSortOrder(string methodName)
        {
            if (methodName.Contains("StreamingSessions")) return 1;
            if (methodName.Contains("TranscoderProfile")) return 1;
            if (methodName.Contains("WebStreamMediaType")) return 1;
            if (methodName.Contains("StreamSize")) return 3;
            if (methodName.Contains("TranscodingInfo")) return 3;
            if (methodName.Contains("MediaInfo")) return 3;
            if (methodName.Contains("CustomTranscoder")) return 5;
            if (methodName.Contains("Artwork")) return 4;
            if (methodName.Contains("Image")) return 4;
            if (methodName.Contains("Stream")) return 2;

            return 1;
        }

        protected override Dictionary<int, string> GetHeadings()
        {
            return new Dictionary<int, string>()
            {
                { 1, "General" },
                { 2, "Control" },
                { 3, "Stream info" },
                { 4, "Images" },
                { 5, "Misc" },
            };
        }
    }
}
