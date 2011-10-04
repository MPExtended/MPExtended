using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MPExtended.Services.MediaAccessService.Trakt
{
    internal enum TraktScrobbleState
    {
        watching,
        scrobble,
        cancelwatching
    }

    internal class TraktAPI
    {
        /// <summary>
        /// Sends Scrobble data to Trakt
        /// </summary>
        /// <param name="scrobbleData">The Data to send</param>
        /// <param name="status">The mode to send it as</param>
        /// <returns>The response from Trakt</returns>
        public static TraktResponse ScrobbleMovie(TraktMovieScrobble scrobbleData, TraktScrobbleState status)
        {
            //If we are cancelling a scrobble we don't need data
            if (status != TraktScrobbleState.cancelwatching)
            {
                // check that we have everything we need
                // server can accept title if movie id is not supplied
                if (scrobbleData == null)
                {
                    TraktResponse error = new TraktResponse
                    {
                        Error = "Not enough information to send to server",
                        Status = "failure"
                    };
                    return error;
                }
            }

            // serialize Scrobble object to JSON and send to server
            string response = Transmit(string.Format(TraktConfig.URL.ScrobbleMovie, status.ToString()), scrobbleData.ToJSON());

            // return success or failure
            return response.FromJSON<TraktResponse>();
        }

        /// <summary>
        /// Sends Scrobble data to Trakt
        /// </summary>
        /// <param name="scrobbleData">The Data to send</param>
        /// <param name="status">The mode to send it as</param>
        /// <returns>The response from Trakt</returns>
        public static TraktResponse ScrobbleEpisodeState(TraktEpisodeScrobble scrobbleData, TraktScrobbleState status)
        {
            // check that we have everything we need
            // server can accept title if movie id is not supplied
            if (status != TraktScrobbleState.cancelwatching)
            {
                if (scrobbleData == null)
                {
                    TraktResponse error = new TraktResponse
                    {
                        Error = "Not enough information to send to server",
                        Status = "failure"
                    };
                    return error;
                }
            }

            // serialize Scrobble object to JSON and send to server
            string response = Transmit(string.Format(TraktConfig.URL.ScrobbleShow, status.ToString()), scrobbleData.ToJSON());

            // return success or failure
            return response.FromJSON<TraktResponse>();
        }

        /// <summary>
        /// Communicates to and from Trakt
        /// </summary>
        /// <param name="address">The URI to use</param>
        /// <param name="data">The Data to send</param>
        /// <returns>The response from Trakt</returns>
        private static string Transmit(string address, string data)
        {
            try
            {
                ServicePointManager.Expect100Continue = false;
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                client.Headers.Add("User-Agent", TraktConfig.UserAgent);
                return client.UploadString(address, data);
            }
            catch (WebException e)
            {
                // something bad happened e.g. invalid login
                TraktResponse error = new TraktResponse
                {
                    Status = "failure",
                    Error = e.Message
                };
                return error.ToJSON();
            }
        }
    }
}
