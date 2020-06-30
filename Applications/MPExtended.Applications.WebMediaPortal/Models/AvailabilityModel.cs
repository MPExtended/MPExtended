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
using System.Web;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class AvailabilityModel
    {
        public bool TAS { get; set; }
        public bool MAS { get; set; }

        public bool Movies { get; set; }
        public bool TVShows { get; set; }
        public bool Music { get; set; }
        public bool Picture { get; set; }

        public bool Authentication { get; set; }

        public AvailabilityModel()
        {
            Reload();
        }

        public void Reload()
        {
            Authentication = Configuration.Authentication.Enabled;

            TAS = Connections.Current.HasTASConnection;
            MAS = Connections.Current.HasMASConnection;

            var msd = Connections.Current.HasMASConnection ? Connections.Current.MAS.GetServiceDescription() : null;
            Movies = MAS &&  (Settings.ActiveSettings.MovieProvider == null ? 
                              msd.DefaultMovieLibrary != 0 : 
                              msd.AvailableMovieLibraries.Any(x => x.Id == Settings.ActiveSettings.MovieProvider));
            TVShows = MAS && (Settings.ActiveSettings.TVShowProvider == null ?
                              msd.DefaultTvShowLibrary != 0 :
                              msd.AvailableTvShowLibraries.Any(x => x.Id == Settings.ActiveSettings.TVShowProvider));
            Music = MAS &&   (Settings.ActiveSettings.MusicProvider == null ?
                              msd.DefaultMusicLibrary != 0 :
                              msd.AvailableMusicLibraries.Any(x => x.Id == Settings.ActiveSettings.MusicProvider));
            Picture = MAS && (Settings.ActiveSettings.PicturesProvider == null ?
                              msd.DefaultPictureLibrary != 0 :
                              msd.AvailablePictureLibraries.Any(x => x.Id == Settings.ActiveSettings.PicturesProvider));
        }
    }
}