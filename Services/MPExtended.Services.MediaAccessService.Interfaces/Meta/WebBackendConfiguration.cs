using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Meta
{
    public class WebBackendConfiguration
    {
        public List<WebBackendProvider> AvailableMovieLibraries { get; set; }
        public List<WebBackendProvider> AvailableMusicLibraries { get; set; }
        public List<WebBackendProvider> AvailablePictureLibraries { get; set; }
        public List<WebBackendProvider> AvailableTvShowLibraries { get; set; }
        public List<WebBackendProvider> AvailableFileSystemLibraries { get; set; }
    }
}
