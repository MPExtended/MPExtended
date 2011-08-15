using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.PlugIns.MAS.MPTVSeries
{
    [Export(typeof(ITVShowLibrary))]
    [ExportMetadata("Database", "MPTVSeries")]
    public class MPTVSeries : ITVShowLibrary
    {
        public IList<WebTVShowBasic> GetAllTVShows()
        {
            throw new NotImplementedException();
        }

        public WebTVShowDetailed GetTVShowDetailed(string seriesId)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVSeasonBasic> GetSeasons(string seriesId)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVEpisodeBasic> GetEpisodes(string seriesId)
        {
            throw new NotImplementedException();
        }

        public IList<WebTVEpisodeBasic> GetEpisodesForSeason(string seriesId, string seasonId)
        {
            throw new NotImplementedException();
        }

        public WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId)
        {
            throw new NotImplementedException();
        }

        public System.IO.DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }
    }
}
