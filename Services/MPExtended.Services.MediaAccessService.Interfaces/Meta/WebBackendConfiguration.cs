using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Meta
{
    public class WebBackendConfiguration
    {
        public List<WebBackendProvider> AvailableMovieProvider { get; set; }
        public List<WebBackendProvider> AvailableMusicProvider { get; set; }
        public List<WebBackendProvider> AvailablePictureProvider { get; set; }
        public List<WebBackendProvider> AvailableTvShowProvider { get; set; }

        public string CurrentMovieProvider { get; set; }
        public string CurrentMusicProvider { get; set; }
        public string CurrentPictureProvider { get; set; }
        public string CurrentTvShowProvider { get; set; }
    }
}
