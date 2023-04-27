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

using System.Collections.Generic;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public interface IActors
    {
        IList<WebActor> Actors { get; set; }
    }

    public interface IMovieActors
    {
        IList<WebMovieActor> Actors { get; set; }
    }

    public interface ITVShowActors
    {
        IList<WebTVShowActor> Actors { get; set; }
    }

    public interface IGuestStars
    {
        IList<WebTVShowActor> GuestStars { get; set; }
    }

    public interface IArtwork
    {
        IList<WebArtwork> Artwork { get; set; }
    }

    public interface IExternalId
    {
        IList<WebExternalId> ExternalIds { get; set; }
    }

    public interface ICollections
    {
      IList<WebCollection> Collections { get; set; }
    }

    public interface IArtists
    {
      IList<WebMusicArtistBasic> Artists { get; set; }
    }

    public interface IAlbumArtist
    {
      WebMusicArtistBasic AlbumArtistObject { get; set; }
  }
}
