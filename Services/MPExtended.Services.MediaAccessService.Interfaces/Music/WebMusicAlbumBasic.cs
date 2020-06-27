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
    public class WebMusicAlbumBasic : WebObject, IRatingSortable, ITitleSortable, IDateAddedSortable, IYearSortable, IGenreSortable, IMusicComposerSortable, IArtwork
    {
        public WebMusicAlbumBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Genres = new List<string>();
            Artists = new List<string>();
            ArtistsId = new List<string>();
            Composer = new List<string>();
            Artwork = new List<WebArtwork>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public IList<string> Genres { get; set; }
        public string AlbumArtist { get; set; }
        public string AlbumArtistId { get; set; }
        public IList<string> Artists { get; set; }
        public IList<string> ArtistsId { get; set; }
        public IList<string> Composer { get; set; }
        public DateTime DateAdded { get; set; }
        public int Year { get; set; }
        public float Rating { get; set; }
        public IList<WebArtwork> Artwork { get; set; }
        public string Codec { get; set; }

        public WebMediaType Type 
        {
            get
            {
                return WebMediaType.MusicAlbum;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
