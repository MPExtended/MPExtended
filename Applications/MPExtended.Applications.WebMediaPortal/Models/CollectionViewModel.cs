#region Copyright (C) 2020 Team MediaPortal
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
using System.Web;
using System.Web.Script.Serialization;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Applications.WebMediaPortal.Models
{
  public class CollectionViewModel
  {

    public WebCollection Collection { get; set; }
    public IEnumerable<WebMovieDetailed> Movies { get; set; }

    public CollectionViewModel(WebCollection collection, IEnumerable<WebMovieDetailed> movies)
    {
      Collection = collection;
      Movies = movies;
    }

    public CollectionViewModel(string Id)
    {
      try
      {
        Collection = Connections.Current.MAS.GetCollectionById(Settings.ActiveSettings.MovieProvider, Id);
        Movies = Connections.Current.MAS.GetMoviesDetailed(Settings.ActiveSettings.MovieProvider, null, WebSortField.Title, WebSortOrder.Asc)
                 .Where(x => x.Collections.Contains(Id));
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load Collection {0}", Id), ex);
      }
    }

  }
}