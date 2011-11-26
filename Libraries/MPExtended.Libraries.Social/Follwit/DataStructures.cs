using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Libraries.Social.Follwit
{
    public enum FollwitWatchStatus
    {
        Watching,
        CancelWatching,
        Watched
    }

    [DataContract]
    internal class FollwitAccountTestData
    {
        [DataMember(Name = "username")]
        public string UserName { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }

    [DataContract]
    internal class FollwitEpisode
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }
        [DataMember(Name = "password")]
        public string Password { get; set; }
        [DataMember(Name = "tvdb_episode_id")]
        public string TVDBId { get; set; }
    }

    [DataContract]
    internal class FollwitMovie
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }
        [DataMember(Name = "password")]
        public string Password { get; set; }
        [DataMember(Name = "imdb_id")]
        public string IMDBId { get; set; }
    }

    [DataContract]
    internal class FollwitResponse
    {
        [DataMember(Name = "response")]
        public string Response { get; set; }
    }
}
