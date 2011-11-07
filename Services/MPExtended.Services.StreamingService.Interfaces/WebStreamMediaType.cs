using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MPExtended.Services.StreamingService.Interfaces
{
    [DataContract]
    public enum WebStreamMediaType
    {
        // should equal to MPExtended.MediaAccessService.Interfaces.WebMediaType, except for TV and Recording values
        [EnumMember]
        TV = -2,
        [EnumMember]
        Recording = -1,
        [EnumMember]
        Movie = 0,
        [EnumMember]
        MusicTrack = 1,
        [EnumMember]
        Picture = 2,
        [EnumMember]
        TVEpisode = 3,
        [EnumMember]
        File = 4,
        [EnumMember]
        TVShow = 5,
        [EnumMember]
        TVSeason = 6,
        [EnumMember]
        MusicAlbum = 7,
        [EnumMember]
        MusicArtist = 8,
        [EnumMember]
        Folder = 9,
        [EnumMember]
        Drive = 10
    }
}
