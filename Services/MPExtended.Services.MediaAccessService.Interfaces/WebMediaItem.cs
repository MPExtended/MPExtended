﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebMediaItem : WebObject, IDateAddedSortable, ITitleSortable, ITypeSortable, IArtwork
    {
        public WebMediaItem()
        {
            DateAdded = new DateTime(1970, 1, 1);
            Path = new List<string>();
            Artwork = new List<WebArtwork>();
        }

        public string Id { get; set; }
        public IList<string> Path { get; set; }
        public DateTime DateAdded { get; set; }
        public string Title { get; set; }
        public IList<WebArtwork> Artwork { get; set; }
        public virtual WebMediaType Type { get; set; }
    }
}