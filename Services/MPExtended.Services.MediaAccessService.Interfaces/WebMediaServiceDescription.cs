using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebMediaServiceDescription
    {
        public int MovieApiVersion { get; set; }
        public int MusicApiVersion { get; set; }
        public int PicturesApiVersion { get; set; }
        public int TvShowsApiVersion { get; set; }
        public int FilesystemApiVersion { get; set; }

        public string ServiceVersion { get; set; }

        public List<WebBackendProvider> AvailableMovieLibraries { get; set; }
        public List<WebBackendProvider> AvailableMusicLibraries { get; set; }
        public List<WebBackendProvider> AvailablePictureLibraries { get; set; }
        public List<WebBackendProvider> AvailableTvShowLibraries { get; set; }
        public List<WebBackendProvider> AvailableFileSystemLibraries { get; set; }

        public int DefaultMovieLibrary { get; set; }
        public int DefaultMusicLibrary { get; set; }
        public int DefaultPictureLibrary { get; set; }
        public int DefaultTvShowLibrary { get; set; }
        public int DefaultFileSystemLibrary { get; set; }
    }
}