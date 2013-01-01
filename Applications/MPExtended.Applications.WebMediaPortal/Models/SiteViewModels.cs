#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class MenuModel
    {
        public IEnumerable<string> MovieGenres
        {
            get
            {
                try
                {
                    return Connections.Current.MAS.GetMovieGenres(Settings.ActiveSettings.MovieProvider)
                        .Select(x => x.Title)
                        .ToList(); // Needed to force execution here, instead of outside the try/catch later on
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load movie genres", ex);
                    return new List<string>();
                }
            }
        }

        public IEnumerable<string> TVShowGenres
        {
            get
            {
                try
                {
                    return Connections.Current.MAS.GetTVShowGenres(Settings.ActiveSettings.TVShowProvider)
                        .Select(x => x.Title)
                        .ToList(); // Needed to force execution here, instead of outside the try/catch later on
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load TVShow genres", ex);
                    return new List<string>();
                }
            }
        }
    }
}
