#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using System.Xml;
using System.IO;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Libraries.ServiceLib.DB;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;


namespace MPExtended.PlugIns.MAS.MPVideos
{
    internal class MPVideoDB : Database
    {

        public MPVideoDB()
            : base(Configuration.GetMPDbLocations().Videos)
        {
        }

        #region Video DB
        public List<WebMovieBasic> GetAllVideos()
        {
            return GetVideos(null, null);
        }

        public List<WebMovieBasic> GetVideos(int? _start, int? _end)
        {
            string sql = "Select movie.idMovie, local.strFilename, path.strPath,info.strTitle, info.strPlotOutline, info.iYear, info.strGenre from movie as movie Natural Join movieinfo as info NATURAL JOIN files as local NATURAL JOIN path as path";
            return ReadList<WebMovieBasic>(sql, delegate(SQLiteDataReader reader)
            {
                try
                {
                    int id = DatabaseHelperMethods.SafeInt32(reader, 0);
                    string title = DatabaseHelperMethods.SafeStr(reader, 3);
                    return new WebMovieBasic()
                    {
                        Id = id,
                        Title = title,
                        TagLine = DatabaseHelperMethods.SafeStr(reader, 4),
                        Year = DatabaseHelperMethods.SafeInt32(reader, 5),
                        Genre = DatabaseHelperMethods.SafeStr(reader, 6),
                        CoverThumbPath = GetCoverArtName(Path.Combine(Utils.GetBannerPath("videos"), "Title"), title + "{" + id + "}")
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
            return GetAllVideos().Count;
        }

        public List<WebMovieBasic> SearchForVideo(String searchString)
        {
            return null;
        }

        public WebMovieDetailed GetFullVideo(int videoId)
        {
            string sql = "Select movie.idMovie, local.strFilename, path.strPath, movie.discid, movie.hasSubtitles, " +
                        "info.strTitle, info.strCast, " +
                        "info.iYear, info.strGenre, info.mpaa, info.strPlotOutline, info.strPlot, info.fRating, info.strVotes, info.runtime, info.IMDBID " +
                        "from movie as movie, movieinfo as info, files as local, path as path where movie.idMovie = info.idMovie and movie.idPath = local.idPath " +
                        "and path.idPath = local.idPath and movie.idMovie = " + videoId;
            return ReadRow<WebMovieDetailed>(sql, delegate(SQLiteDataReader reader)
            {
                try
                {
                    int id = DatabaseHelperMethods.SafeInt32(reader, 0);
                    string title = DatabaseHelperMethods.SafeStr(reader, 5);
                    WebMovieDetailed movie = new WebMovieDetailed()
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
                        CoverThumbPath = GetCoverArtName(Path.Combine(Utils.GetBannerPath("videos"), "Title"), title + "{" + id + "}"),
                        CoverPath = GetLargeCoverArtName(Path.Combine(Utils.GetBannerPath("videos"), "Title"), title + "{" + id + "}")
                    };

                    movie.Files.Add(new WebMovieFile()
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

        #region Banners
        public static string GetCoverArtName(string strFolder, string strFileName)
        {
            if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
                return string.Empty;

            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                strFileName = strFileName.Replace(c, '_');

            return String.Format(@"{0}\{1}{2}", strFolder, strFileName, ".jpg");
        }

        public static string GetLargeCoverArtName(string strFolder, string strFileName)
        {
            if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
                return string.Empty;

            return GetCoverArtName(strFolder, strFileName + "L");
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
                WebMovieDetailed vid = GetFullVideo(Int32.Parse(itemId));
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