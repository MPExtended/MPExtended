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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
    public class WebMovieBasic : WebMediaItem, IYearSortable, IGenreSortable, IRatingSortable, IActors
    {
        public WebMovieBasic()
        {
            Genres = new List<string>();
            ExternalId = new List<WebExternalId>();
            Actors = new List<WebActor>();
            Groups = new List<string>();
            Collections = new List<WebCollection>();
        }

        public bool IsProtected { get; set; }
        public IList<string> Genres { get; set; }
        public IList<WebExternalId> ExternalId { get; set; }
        public IList<WebActor> Actors { get; set; }
        public IList<string> Groups { get; set; }
        public IList<WebCollection> Collections { get; set; }

        public int Year { get; set; }
        public float Rating { get; set; }
        public int Runtime { get; set; }

        public string MPAARating { get; set; }

        public bool Watched { get; set; }
        public int TimesWatched { get; set; }
        public string Stoptime { get; set; }

        public override WebMediaType Type 
        {
            get
            {
                return WebMediaType.Movie;
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
