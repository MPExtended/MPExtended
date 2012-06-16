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
using System.Web;
using System.Web.Script.Serialization;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Client;
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class MovieViewModel
    {
        private WebFileInfo fileInfo;
        private WebMediaInfo mediaInfo;

        public WebMovieDetailed Movie { get; set; }

        [ScriptIgnore]
        public string Id { get; set; }

        // Most of these properties below probably violate the design guidelines by being a property: they are too slow. However,
        // if I make them a method they won't be properly serialized by the JavaScriptSerializer, so I have to do it this way.
        // (the alternative, a field, is even worse as it has to be public). 

        public WebFileInfo FileInfo
        {
            get
            {
                if (fileInfo == null)
                    fileInfo = MPEServices.MAS.GetFileInfo(Movie.PID, WebMediaType.Movie, WebFileType.Content, Movie.Id, 0);

                return fileInfo;
            }
        }

        public WebMediaInfo MediaInfo
        {
            get
            {
                if (mediaInfo == null)
                    mediaInfo = MPEServices.MASStreamControl.GetMediaInfo(WebStreamMediaType.Movie, Movie.PID, Movie.Id);

                return mediaInfo;
            }
        }

        public string Quality
        {
            get
            {
                return MediaInfoFormatter.GetShortQualityName(MediaInfo);
            }
        }

        public string FullQuality
        {
            get
            {
                return MediaInfoFormatter.GetFullInfoString(MediaInfo, FileInfo);
            }
        }

        public MovieViewModel(WebMovieDetailed movie)
        {
            Movie = movie;
            Id = Movie.Id;
        }

        public MovieViewModel(string id)
        {
            try
            {
                Movie = MPEServices.MAS.GetMovieDetailedById(Settings.ActiveSettings.MovieProvider, id);
                Id = Movie.Id;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to load movie {0}", id), ex);
            }
        }
    }
}