#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Net;
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Social.Trakt
{
    // This implementation is inspired by the TraktAPI part of the Trakt for MediaPortal plugin, as can be found
    // here: https://github.com/Technicolour/Trakt-for-Mediaportal. Thanks a lot!

    internal class TraktAPI
    {
        public static TraktResponse ScrobbleMovie(TraktMovieScrobbleData data, TraktWatchStatus status)
        {
            string url = String.Format(TraktConfig.URL.ScrobbleMovie, MapToURL(status));
            string json = CallAPI(url, JSONUtil.ToJSON(data));
            return JSONUtil.FromJSON<TraktResponse>(json);
        }

        public static TraktResponse ScrobbleEpisode(TraktEpisodeScrobbleData data, TraktWatchStatus status)
        {
            string url = String.Format(TraktConfig.URL.ScrobbleShow, MapToURL(status));
            string json = CallAPI(url, JSONUtil.ToJSON(data));
            return JSONUtil.FromJSON<TraktResponse>(json);
        }

        public static TraktResponse TestAccount(TraktAccountTestData data)
        {
            string url = TraktConfig.URL.TestAccount;
            string json = CallAPI(url, JSONUtil.ToJSON(data));
            return JSONUtil.FromJSON<TraktResponse>(json);
        }

        private static string MapToURL(TraktWatchStatus status)
        {
            switch (status)
            {
                case TraktWatchStatus.CancelWatching:
                    return "cancelwatching";
                case TraktWatchStatus.Scrobble:
                    return "scrobble";
                case TraktWatchStatus.Watching:
                    return "watching";
                default:
                    throw new ArgumentException();
            }
        }

        private static string CallAPI(string address, string data)
        {
            WebClient client = new WebClient();
            try
            {
                ServicePointManager.Expect100Continue = false;
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("User-Agent", TraktConfig.UserAgent);
                return client.UploadString(address, data);
            }
            catch (WebException e)
            {
                // this might happen in the TestAccount method
                if ((e.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                {
                    using (var reader = new StreamReader(e.Response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }

                Log.Warn("Failed to call Trakt API", e);
                return String.Empty;
            }
        }
    }
}
