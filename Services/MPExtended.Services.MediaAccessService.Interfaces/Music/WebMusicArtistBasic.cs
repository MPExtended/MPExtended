#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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

using MPExtended.Services.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Music
{
    public class WebMusicArtistBasic : WebObject, ITitleSortable, IArtwork
    {
        public WebMusicArtistBasic()
        {
            Artwork = new List<WebArtwork>();
            HasAlbums = true;//default
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public bool HasAlbums { get; set; }
        public IList<WebArtwork> Artwork { get; set; }

        public WebMediaType Type 
        {
            get
            {
                return WebMediaType.MusicArtist;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
