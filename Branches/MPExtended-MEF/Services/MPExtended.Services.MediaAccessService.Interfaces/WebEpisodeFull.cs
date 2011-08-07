using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebEpisodeFull : WebEpisode
    {
        /// <summary>
        /// Some Episodes may consist of more than one file
        /// Each WebEpisodeFile represents a physical file on disk
        /// 
        /// Database: local_episodes, comments are field names in database
        /// </summary>
        public class WebEpisodeFile
        {
            public String FileName { get; set; }//EpisodeFileName
            public int EpisodeIndex { get; set; }//EpisodeIndex
            public int SeasonIndex { get; set; }//SeasonIndex
            public bool IsAvailable { get; set; }//IsAvailable
            public bool IsRemovable { get; set; }//Removable
            public int Duration { get; set; }//localPlaytime
            public int VideoWidth { get; set; }//videoWidth
            public int VideoHeight { get; set; }//videoHeight

            public String VideoCodec { get; set; }//VideoCodec
            public int VideoBitrate { get; set; }//VideoBitrate
            public float VideoFrameRate { get; set; }//VideoFrameRate
            public String AudioCodec { get; set; }//AudioCodec
            public int AudioBitrate { get; set; }//AudioBitrate
            public int AudioChannels { get; set; }//AudioChannels
            public int AudioTracks { get; set; }//AudioTracks
            public bool HasSubtitles { get; set; } //AvailableSubtitles

        }

        //Div Info
        public String Summary { get; set; } //Summary
        public String GuestStarsString { get; set; } //GuestStars
        public List<String> GuestStars { get; set; }
        public String DirectorsString { get; set; } //Director
        public List<String> Directors { get; set; }
        public String WritersString { get; set; } //Writer
        public List<String> Writers { get; set; }
        public DateTime LastUpdated { get; set; } //lastupdated
        public String ImdbId { get; set; }//IMDB_ID
        public String ProductionCode { get; set; }//ProductionCode

        //Combined 
        public int CombinedEpisodeNumber { get; set; } //Combined_episodenumber
        public int CombinedSeasonNumber { get; set; } //Combined_season

        //DVD
        public int DvdChapter { get; set; } //DVD_chapter
        public int DvdDiscid { get; set; } //DVD_discid
        public int DvdEpisodenumber { get; set; } //DVD_episodenumber
        public int DvdSeason { get; set; } //DVD_season

        //absolute ordering
        public int AbsoluteEpisodeNumber { get; set; }//absolute_number

        //Specials
        public int AirsAfterSeason { get; set; }//airsafter_season
        public int AirsBeforeEpisode { get; set; }//airsbefore_episode
        public int AirsBeforeSesaon { get; set; }//airsbefore_season

        //Physical files
        public WebEpisodeFile EpisodeFile { get; set; }//The physical file on disk
        public WebEpisodeFile EpisodeFile2 { get; set; }//Episode consists of a second file
    }
}