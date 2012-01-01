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
using System.Net;
using System.Security.Cryptography;
using MPExtended.Libraries.General;

namespace MPExtended.Libraries.Social.Follwit
{
    internal class FollwitAPI
    {
        // This implementation is based upon the one from the follw.it guys theirselves, which can be found 
        // here: http://svn.follw.it/api/trunk/. The authentication code is literally copied from their
        // apidocs: http://follw.it/apidocs/authentication

        public static string GeneratePasswordHash(string password)
        {
            // salt + hash
            string salt = "52c3a0d0-f793-46fb-a4c0-35a0ff6844c8";
            string saltedPassword = password + salt;
            string sHash = "";

            SHA1CryptoServiceProvider sha1Obj = new SHA1CryptoServiceProvider();
            byte[] bHash = sha1Obj.ComputeHash(Encoding.ASCII.GetBytes(saltedPassword));

            foreach (byte b in bHash)
                sHash += b.ToString("x2");
            return sHash;
        }

        public static FollwitResponse UpdateMovieState(FollwitMovie data, FollwitWatchStatus status)
        {
            string url = String.Format(FollwitConfig.URL.WatchMovie, MapToURL(status));
            string json = CallAPI(url, JSONUtil.ToJSON(data));
            return JSONUtil.FromJSON<FollwitResponse>(json);
        }

        public static FollwitResponse UpdateEpisodeState(FollwitEpisode data, FollwitWatchStatus status)
        {
            string url = String.Format(FollwitConfig.URL.WatchEpisode, MapToURL(status));
            string json = CallAPI(url, JSONUtil.ToJSON(data));
            return JSONUtil.FromJSON<FollwitResponse>(json);
        }

        public static FollwitResponse TestAccount(FollwitAccountTestData data)
        {
            string json = CallAPI(FollwitConfig.URL.TestAccount, JSONUtil.ToJSON(data));
            return JSONUtil.FromJSON<FollwitResponse>(json);
        }

        private static string MapToURL(FollwitWatchStatus status)
        {
            switch (status)
            {
                case FollwitWatchStatus.CancelWatching:
                    return "unwatching";
                case FollwitWatchStatus.Watching:
                    return "watching";
                case FollwitWatchStatus.Watched:
                    return "watched";
                default:
                    throw new ArgumentException();
            }
        }

        private static string CallAPI(string address, string data)
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("User-Agent", FollwitConfig.UserAgent);
                return client.UploadString(address, data);
            }
            catch (WebException e)
            {
                Log.Warn("Failed to call Follwit API", e);
                return String.Empty;
            }
        }
    }
}
