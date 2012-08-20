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
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.PlugIns.MAS.MPShares
{
    internal class Share
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Id { get; set; }
        public List<string> Extensions { get; set; }

        public WebDriveBasic ToWebDriveBasic()
        {
            return new WebDriveBasic()
            {
                Title = this.Name,
                Path = new List<string>() { this.Path },
                Id = this.Id,
                LastModifiedTime = DateTime.Now,
                LastAccessTime = DateTime.Now
            };
        }
    }
}
