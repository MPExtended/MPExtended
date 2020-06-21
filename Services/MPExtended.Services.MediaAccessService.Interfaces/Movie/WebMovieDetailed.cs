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

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
    public class WebMovieDetailed : WebMovieBasic
    {
        public WebMovieDetailed() : base()
        {
            Directors = new List<string>();
            Writers = new List<string>();
            Collections = new List<string>();
            Groups = new List<string>();
            Studios = new List<string>();
        }

        public IList<string> Directors { get; set; }
        public IList<string> Writers { get; set; }
        public string Summary { get; set; }
        public string Tagline { get; set; }
    
        // use ISO short name (en, nl, de, etc)
        public string Language { get; set; }

        public IList<string> Collections { get; set; }
        public IList<string> Groups { get; set; }

        public string MPAARating { get; set; }
        public string MPAAText { get; set; }
        public string Awards { get; set; }

        public IList<string> Studios { get; set; }
  }
}
