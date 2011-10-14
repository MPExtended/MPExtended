using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    [DataContract]
    public enum WebMediaType
    {
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
    }

    [DataContract]
    public enum WebFileType
    {
        [EnumMember]
        Content = 0,
        [EnumMember]
        Backdrop = 1,
        [EnumMember]
        Banner = 2,
        [EnumMember]
        Poster = 3,
        [EnumMember]
        Cover = 4

        // 5 is reserved for tv logos in the streaming service
    }

    [DataContract]
    public enum OrderBy
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1
    }

    [DataContract]
    public enum SortBy
    {
        [EnumMember]
        Title = 0,
        [EnumMember]
        DateAdded = 1,
        [EnumMember]
        Year = 2,
        [EnumMember]
        Genre = 3,
        [EnumMember]
        Rating = 4,
        [EnumMember]
        UserDefinedCategories = 5,
        [EnumMember]
        MusicTrackNumber = 6,
        [EnumMember]
        MusicComposer = 7,
        [EnumMember]
        TVEpisodeNumber = 8,
        [EnumMember]
        TVSeasonNumber = 9,
        [EnumMember]
        PictureDateTaken = 10,
        [EnumMember]
        TVDateAired = 11
    }
}
