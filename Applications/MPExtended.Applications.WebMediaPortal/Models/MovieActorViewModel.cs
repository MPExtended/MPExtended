﻿#region Copyright (C) 2020 Team MediaPortal
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

using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Applications.WebMediaPortal.Models
{
  public class MovieActorViewModel
  {

    public WebMovieActor Actor { get; set; }
    public IEnumerable<WebMovieDetailed> Movies { get; set; }

    public MovieActorViewModel(WebMovieActor actor, IEnumerable<WebMovieDetailed> movies)
    {
      Actor = actor;
      Movies = movies;
    }

    public MovieActorViewModel(string Id)
    {
      try
      {
        Actor = Connections.Current.MAS.GetMovieActorById(Settings.ActiveSettings.MovieProvider, Id);
        Movies = Connections.Current.MAS.GetMoviesDetailed(Settings.ActiveSettings.MovieProvider, null, WebSortField.Title, WebSortOrder.Asc)
                 .Where(x => x.Actors.Contains(Id));
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load Actor {0}", Id), ex);
      }
    }

  }
}