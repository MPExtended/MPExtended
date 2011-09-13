#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.PlugIns.MAS.MPVideos
{
    [Export(typeof(IMovieLibrary))]
    [ExportMetadata("Database", "MPMyVideo")]
    public class MPVideos : IMovieLibrary
    {
        private IPluginData data;

        [ImportingConstructor]
        public MPVideos(IPluginData data)
        {
            this.data = data;
        }

        public IEnumerable<WebMovieBasic> GetAllMovies()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebMovieDetailed> GetAllMoviesDetailed()
        {
            throw new NotImplementedException();
        }

        public WebMovieBasic GetMovieBasicById(string movieId)
        {
            throw new NotImplementedException();
        }

        public WebMovieDetailed GetMovieDetailedById(string movieId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebGenre> GetAllGenres()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            throw new NotImplementedException();
        }

        public DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }


        public Stream GetCover(string movieId, int offset)
        {
            throw new NotImplementedException();
        }

        public Stream GetBackdrop(string movieId, int offset)
        {
            throw new NotImplementedException();
        }

        public bool IsLocalFile(string path)
        {
            throw new NotImplementedException();
        }

        public Stream GetFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
