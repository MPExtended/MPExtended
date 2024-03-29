﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeDetailed : WebTVEpisodeBasic, IGuestStars
    {
        public WebTVEpisodeDetailed() : base()
        {
            GuestStars = new List<WebTVShowActor>();
            Directors = new List<string>();
            Writers = new List<string>();
        }

        public IList<WebTVShowActor> GuestStars { get; set; }
        public IList<string> Directors { get; set; }
        public IList<string> Writers { get; set; }

        public string Show { get; set; }
        public string Summary { get; set; }
    }
}
