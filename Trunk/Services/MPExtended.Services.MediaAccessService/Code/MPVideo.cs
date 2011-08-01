using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using System.Xml;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Code.Helper;

namespace MPExtended.Services.MediaAccessService.Code
{
    internal class MPVideo : Database
    {

        public MPVideo()
            : base(Utils.GetMPDbLocations().Videos)
        {
        }

        #region Video DB
        public List<WebMovie> GetAllVideos()
        {
            return GetVideos(null, null);
        }

        public List<WebMovie> GetVideos(int? _start, int? _end)
        {
            string sql = "Select movie.idMovie, local.strFilename, path.strPath,info.strTitle, info.strPlotOutline, info.iYear, info.strGenre from movie as movie Natural Join movieinfo as info NATURAL JOIN files as local NATURAL JOIN path as path";
            return ReadList<WebMovie>(sql, delegate(SQLiteDataReader reader)
            {
                try
                {
                    int id = DatabaseHelperMethods.SafeInt32(reader, 0);
                    string title = DatabaseHelperMethods.SafeStr(reader, 3);
                    return new WebMovie()
                    {
                        Id = id,
                        Title = title,
                        TagLine = DatabaseHelperMethods.SafeStr(reader, 4),
                        Year = DatabaseHelperMethods.SafeInt32(reader, 5),
                        Genre = DatabaseHelperMethods.SafeStr(reader, 6),
                        CoverThumbPath = Utils.GetCoverArtName(@"C:\ProgramData\Team MediaPortal\MediaPortal\thumbs\Videos\Title", title + "{" + id + "}")
                    };
                }
                catch (Exception ex)
                {
                    Log.Error("Error reading video", ex);
                    return null;
                }
            }, _start, _end);
        }

        public int GetVideosCount()
        {
            return ReadInt("Select COUNT(movie.idMovie) " +
                           "from movie as movie, movieinfo as info where movie.idMovie = info.idMovie");
        }

        public List<WebMovie> SearchForVideo(String searchString)
        {
            return null;
        }

        public WebMovieFull GetFullVideo(int videoId)
        {
            string sql = "Select movie.idMovie, local.strFilename, path.strPath, movie.discid, movie.hasSubtitles, " +
                        "info.strTitle, info.strCast, " +
                        "info.iYear, info.strGenre, info.mpaa, info.strPlotOutline, info.strPlot, info.fRating, info.strVotes, info.runtime, info.IMDBID " +
                        "from movie as movie, movieinfo as info, files as local, path as path where movie.idMovie = info.idMovie and movie.idPath = local.idPath " +
                        "and path.idPath = local.idPath and movie.idMovie = " + videoId;
            return ReadRow<WebMovieFull>(sql, delegate(SQLiteDataReader reader)
            {
                try
                {
                    int id = DatabaseHelperMethods.SafeInt32(reader, 0);
                    string title = DatabaseHelperMethods.SafeStr(reader, 5);
                    WebMovieFull movie = new WebMovieFull()
                    {
                        Id = id,
                        Title = title,
                        Actors = DatabaseHelperMethods.SafeStr(reader, 6),
                        Year = DatabaseHelperMethods.SafeInt32(reader, 7),
                        Genre = DatabaseHelperMethods.SafeStr(reader, 8),
                        Certification = DatabaseHelperMethods.SafeStr(reader, 9),
                        TagLine = DatabaseHelperMethods.SafeStr(reader, 10),
                        Summary = DatabaseHelperMethods.SafeStr(reader, 11),
                        Score = Double.Parse(DatabaseHelperMethods.SafeStr(reader, 12)),
                        Popularity = Int32.Parse(DatabaseHelperMethods.SafeStr(reader, 13)),
                        Runtime = DatabaseHelperMethods.SafeInt32(reader, 14),
                        ImdbId = DatabaseHelperMethods.SafeStr(reader, 15),
                        CoverThumbPath = Utils.GetCoverArtName(@"C:\ProgramData\Team MediaPortal\MediaPortal\thumbs\Videos\Title", title + "{" + id + "}"),
                        CoverPath = Utils.GetLargeCoverArtName(@"C:\ProgramData\Team MediaPortal\MediaPortal\thumbs\Videos\Title", title + "{" + id + "}")
                    };

                    movie.Files.Add(new WebMovieFull.WebMovieFile()
                    {
                        Filename = DatabaseHelperMethods.SafeStr(reader, 1) + DatabaseHelperMethods.SafeStr(reader, 2),
                        DiscId = DatabaseHelperMethods.SafeStr(reader, 3),
                        HasSubtitles = DatabaseHelperMethods.SafeBoolean(reader, 4),
                    });

                    return movie;
                }
                catch (Exception ex)
                {
                    Log.Error("Error reading video details", ex);
                    return null;
                }
            });
        }
        #endregion

        #region Video Path
        /// <summary>
        /// Gets the path to a media item
        /// </summary>
        /// <param name="itemId">Id of the media item</param>
        /// <returns>Path to the mediaitem or null if item id doesn't exist</returns>
        internal string GetVideoPath(string itemId)
        {
            try
            {
                WebMovieFull vid = GetFullVideo(Int32.Parse(itemId));
                if (vid != null)
                {
                    return vid.Files.First().Filename;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting video path for " + itemId, ex);
            }
            return null;
        }
        #endregion
    }
}