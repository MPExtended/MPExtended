﻿#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
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
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class MenuModel
    {
        private static IEnumerable<string> musicGenresCache;

        private static IEnumerable<string> tvShowGenresCache;

        private static IEnumerable<string> movieGenresCache;
        private static IEnumerable<string> movieCategoriesCache;
        private static IEnumerable<string> movieCollectionsCache;

        private RouteData routeData;

        public MenuModel(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public string MusicControllerMethod
        {
            get { return Configuration.WebMediaPortal.MusicLayout.ToString(); }
        }
        
        public IEnumerable<string> MusicGenres
        {
            get
            {
                if (musicGenresCache != null)
                    return musicGenresCache;

                try
                {
                    musicGenresCache = Connections.Current.MAS.GetMusicGenres(Settings.ActiveSettings.MusicProvider)
                        .Select(x => x.Title)
                        .ToList(); // Needed to force execution here, instead of outside the try/catch later on
                    return musicGenresCache;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load music genres", ex);
                    return new List<string>();
                }
            }
        }

        public IEnumerable<string> MovieGenres
        {
            get
            {
                if (movieGenresCache != null)
                    return movieGenresCache;

                try
                {
                    movieGenresCache = Connections.Current.MAS.GetMovieGenres(Settings.ActiveSettings.MovieProvider)
                        .Select(x => x.Title)
                        .ToList(); // Needed to force execution here, instead of outside the try/catch later on
                    return movieGenresCache;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load movie genres", ex);
                    return new List<string>();
                }
            }
        }
        
        public IEnumerable<string> MovieCategories
        {
            get
            {
                if (movieCategoriesCache != null)
                    return movieCategoriesCache;

                try
                {
                    movieCategoriesCache = Connections.Current.MAS.GetMovieCategories(Settings.ActiveSettings.MovieProvider)
                        .Select(x => x.Title)
                        .ToList(); // Needed to force execution here, instead of outside the try/catch later on
                    return movieCategoriesCache;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load movie categories", ex);
                    return new List<string>();
                }
            }
        }

        public IEnumerable<string> MovieCollections
        {
            get
            {
                if (movieCollectionsCache != null)
                    return movieCollectionsCache;

                try
                {
                    movieCollectionsCache = Connections.Current.MAS.GetCollections(Settings.ActiveSettings.MovieProvider)
                        .Select(x => x.Title)
                        .ToList(); // Needed to force execution here, instead of outside the try/catch later on
                    return movieCollectionsCache;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load movie collections", ex);
                    return new List<string>();
                }
            }
        }
        
        public IEnumerable<string> TVShowGenres
        {
            get
            {
                if (tvShowGenresCache != null)
                    return tvShowGenresCache;

                try
                {
                    tvShowGenresCache = Connections.Current.MAS.GetTVShowGenres(Settings.ActiveSettings.TVShowProvider)
                        .Select(x => x.Title)
                        .ToList(); // Needed to force execution here, instead of outside the try/catch later on
                    return tvShowGenresCache;
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to load TVShow genres", ex);
                    return new List<string>();
                }
            }
        }
        
        public bool IsActive(string controller)
        {
            return routeData.Values["controller"].ToString() == controller;
        }

        public bool IsActive(string action, string controller)
        {
            return routeData.Values["controller"].ToString() == controller && routeData.Values["action"].ToString() == action;
        }
    }
}
