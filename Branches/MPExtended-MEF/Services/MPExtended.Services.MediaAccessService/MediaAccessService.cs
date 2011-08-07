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
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Code;
using MPExtended.Services.MediaAccessService.Code.Helper;
using MPExtended.Services.MediaAccessService.Interfaces;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.Services.MediaAccessService
{

    // each method described by IMediaAccessService has to be implemented here, but instead of doing it itself it just references to the MediaInterfaces
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class MediaAccessService : IMediaAccessService
    {
        //[Import]
        //public IMusicLibrary MusicLibrary { get; set; }
        [Import]
        public IMovieLibrary MovieLibrary { get; set; }
        //[Import]
        //public IPictureLibrary PictureLibrary { get; set; }
        // [Import]
        //public ITVShowLibrary TVShowLibrary { get; set; }

        public MediaAccessService()
        {

            AggregateCatalog agrCatalog = new AggregateCatalog(new DirectoryCatalog(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"MPExtended\Extensions\"));
            CompositionContainer objContainer = new CompositionContainer(agrCatalog);
            TypeCatalog moduleCatalog1 = new TypeCatalog(typeof(IMovieLibrary));
            //TypeCatalog moduleCatalog2 = new TypeCatalog(typeof(IMusicLibrary));
            agrCatalog.Catalogs.Add(moduleCatalog1);
            //agrCatalog.Catalogs.Add(moduleCatalog2);
            objContainer.ComposeParts(this);
        }

        public List<WebMovieBasic> GetAllMovies()
        {
            return MovieLibrary.GetAllMovies();
        
        }

 
    }   
}
