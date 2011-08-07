using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebEpisode
    {
        public int IdSerie { get; set; }
        public int Id { get; set; }//EpisodeId
        public String Name { get; set; }//EpisodeName
        public int SeasonNumber { get; set; }//SeasonIndex
        public int EpisodeNumber { get; set; }//EpisodeIndex
        public int Watched { get; set; }//Watched
        public DateTime FirstAired { get; set; }//FirstAired
        public String BannerUrl { get; set; }//thumbFilename
        public double Rating { get; set; }//Rating
        public int RatingCount { get; set; }//How many ratings
        public bool HasLocalFile { get; set; }

        public String FileName { get; set; } //filename of episode
    }
}