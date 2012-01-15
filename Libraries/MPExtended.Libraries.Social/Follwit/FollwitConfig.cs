#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Social.Follwit
{
    internal class FollwitConfig
    {
        private const string ApiKey = "8YJvxv35baL6e";

        public static string UserAgent
        {
            get
            {
                return String.Format("MPExtended {0}", VersionUtil.GetFullVersionString());
            }
        }

        internal static class URL
        {
            public const string WatchEpisode = @"http://follw.it/api/3/" + ApiKey + "/episode.{0}";
            public const string WatchMovie = @"http://follw.it/api/3/" + ApiKey + "/movie.{0}";
            public const string TestAccount = @"http://follw.it/api/3/" + ApiKey + "/user.authenticate";
        }
    }
}
