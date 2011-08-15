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
using System.Data.SQLite;
using System.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Libraries.ServiceLib.DB;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;

namespace MPExtended.PlugIns.MAS.MovingPictures
{
    internal class MovingPicturesDB : Database
    {
        public MovingPicturesDB() :
            base(Configuration.GetMPDbLocations().MovingPictures)
        {
        }

        public WebMovieDetailed GetFullMovie(string idMovie)
        {
            string sql = "SELECT movie.id, local.fullpath, local.discid, local.filehash, local.part, local.duration, local.videowidth, local.videoheight, local.videoresolution, local.videocodec, " +
                         "local.audiocodec, local.audiochannels, local.hassubtitles, local.videoformat, " +
                         "movie.title, movie.alternate_titles, movie.sortby, movie.directors, movie.writers, movie.actors, " +
                         "movie.year, movie.genres, movie.certification, movie.language, movie.tagline, movie.summary, movie.score, movie.popularity, movie.date_added, movie.runtime, movie.imdb_id, " +
                         "movie.alternatecovers, movie.coverfullpath, movie.coverthumbfullpath, movie.backdropfullpath " +
                         "FROM movie_info AS movie " +
                         "INNER JOIN local_media__movie_info AS info ON info.movie_info_id = movie.id " +
                         "INNER JOIN local_media AS local ON local.id = info.local_media_id AND local.ignored = 0 " +
                         "WHERE movie.id = @idMovie";
            WebMovieDetailed movie = new WebMovieDetailed();
            //movie.Files = new List<WebMovieDetailed.WebMovieFile>();
            ReadList<int>(sql, delegate(SQLiteDataReader reader) 
            {
                movie = AddToMovie(reader, movie);
                return 1;
            }, null, null, new SQLiteParameter("@idMovie", idMovie));

            return movie;
        }

        public List<WebMovieDetailed> GetAllMoviesDetailed()
        {
            string sql = "SELECT movie.id, local.fullpath, local.discid, local.filehash, local.part, local.duration, local.videowidth, local.videoheight, local.videoresolution, local.videocodec, " +
                         "local.audiocodec, local.audiochannels, local.hassubtitles, local.videoformat, " +
                         "movie.title, movie.alternate_titles, movie.sortby, movie.directors, movie.writers, movie.actors, " +
                         "movie.year, movie.genres, movie.certification, movie.language, movie.tagline, movie.summary, movie.score, movie.popularity, movie.date_added, movie.runtime, movie.imdb_id, " +
                         "movie.alternatecovers, movie.coverfullpath, movie.coverthumbfullpath, movie.backdropfullpath " +
                         "FROM movie_info AS movie " +
                         "INNER JOIN local_media__movie_info AS info ON info.movie_info_id = movie.id " +
                         "INNER JOIN local_media AS local ON local.id = info.local_media_id AND local.ignored = 0 " +
                         "ORDER BY movie.id";
            Dictionary<int, WebMovieDetailed> ret = new Dictionary<int, WebMovieDetailed>();
            ReadList<int>(sql, delegate(SQLiteDataReader reader)
            {
                int id = DatabaseHelperMethods.SafeInt32(reader, 0);
                if (!ret.ContainsKey(id))
                {
                    ret[id] = new WebMovieDetailed();
                }
                ret[id] = AddToMovie(reader, ret[id]);
                return 1;
            });
            return ret.Select(kpv => kpv.Value).ToList();
        }

        private WebMovieDetailed AddToMovie(SQLiteDataReader reader, WebMovieDetailed movie)
        {
            try
            {
                // set movie properties
                movie.MovieId = DatabaseHelperMethods.SafeInt32(reader, 0).ToString();
                movie.Title = DatabaseHelperMethods.SafeStr(reader, 14);
                //movie.AlternateTitles = DatabaseHelperMethods.SafeStr(reader, 15);
                //movie.SortBy = DatabaseHelperMethods.SafeStr(reader, 16);
                //movie.Directors = DatabaseHelperMethods.SafeStr(reader, 17);
                //movie.Writers = DatabaseHelperMethods.SafeStr(reader, 18);
                //movie.Actors = DatabaseHelperMethods.SafeStr(reader, 19);
                movie.Year = DatabaseHelperMethods.SafeInt32(reader, 20);
                movie.Genre = DatabaseHelperMethods.SafeStr(reader, 21);
                //movie.Certification = DatabaseHelperMethods.SafeStr(reader, 22);
                movie.Language = DatabaseHelperMethods.SafeStr(reader, 23);
                //movie.TagLine = DatabaseHelperMethods.SafeStr(reader, 24);
                movie.Summary = DatabaseHelperMethods.SafeStr(reader, 25);
                movie.Rating =Convert.ToInt32(DatabaseHelperMethods.SafeFloat(reader, 26));
                //movie.Popularity = DatabaseHelperMethods.SafeInt32(reader, 27);
                movie.DateAdded = DatabaseHelperMethods.SafeDateTime(reader, 28);
                //movie.Runtime = DatabaseHelperMethods.SafeInt32(reader, 29);
                //movie.ImdbId = DatabaseHelperMethods.SafeStr(reader, 30);
                //movie.CoverPathAlternate = DatabaseHelperMethods.SafeStr(reader, 31);
                movie.CoverPath = DatabaseHelperMethods.SafeStr(reader, 32);
                movie.CoverThumbPath = DatabaseHelperMethods.SafeStr(reader, 33);
                movie.BackdropPath = DatabaseHelperMethods.SafeStr(reader, 34);

                 //set file properties
                WebMovieFile file = new WebMovieFile();
                file.Filename = DatabaseHelperMethods.SafeStr(reader, 1);
                file.DiscId = DatabaseHelperMethods.SafeStr(reader, 2);
                file.Hash = DatabaseHelperMethods.SafeStr(reader, 3);
                file.Part = DatabaseHelperMethods.SafeInt32(reader, 4);
                file.Duration = DatabaseHelperMethods.SafeInt32(reader, 5);
                file.VideoWidth = DatabaseHelperMethods.SafeInt32(reader, 6);
                file.VideoHeight = DatabaseHelperMethods.SafeInt32(reader, 7);
                file.VideoResolution = DatabaseHelperMethods.SafeStr(reader, 8);
                file.VideoCodec = DatabaseHelperMethods.SafeStr(reader, 9);
                file.AudioCodec = DatabaseHelperMethods.SafeStr(reader, 10);
                file.AudioChannels = DatabaseHelperMethods.SafeStr(reader, 11);
                file.HasSubtitles = DatabaseHelperMethods.SafeBoolean(reader, 12);
                file.VideoFormat = DatabaseHelperMethods.SafeStr(reader, 13);
                movie.Files.Add(file);
                return movie;
            }
            catch (Exception e)
            {
                Log.Warn("Failed to load movie details", e);
                return movie;
            }
        }

        public int GetMovieCount()
        {
            return GetAllMovies().Count;
        }

        public List<WebMovieBasic> GetAllMovies()
        {
            return GetMovies(null, null);
        }

        public List<WebMovieBasic> GetMovies(int? start, int? end)
        {
            string sql = "SELECT DISTINCT movie.id, movie.title, movie.tagline, movie.year, movie.genres, movie.coverthumbfullpath, movie.backdropfullpath " +
                         "FROM movie_info movie " +
                         "INNER JOIN local_media__movie_info AS info ON info.movie_info_id = movie.id " +
                         "INNER JOIN local_media AS local ON local.id = info.local_media_id AND local.ignored = 0 " +
                         "ORDER BY movie.id ";
            return ReadList<WebMovieBasic>(sql, FillBasicMovie, start, end);
        }

        public List<WebMovieBasic> SearchForMovie(String searchString)
        {
            string sql = "SELECT DISTINCT movie.id, movie.title, movie.tagline, movie.year, movie.genres, movie.coverthumbfullpath, movie.backdropfullpath " +
                         "FROM movie_info movie " +
                         "INNER JOIN local_media__movie_info AS info ON info.movie_info_id = movie.id " +
                         "INNER JOIN local_media AS local ON local.id = info.local_media_id AND local.ignored = 0 " +
                         "WHERE movie.title LIKE @search " +
                         "ORDER BY movie.id ";
            return ReadList<WebMovieBasic>(sql, FillBasicMovie, null, null, new SQLiteParameter("@search", String.Format("%{0}%", searchString)));
        }

        private WebMovieBasic FillBasicMovie(SQLiteDataReader reader)
        {
            try
            {
                return new WebMovieBasic()
                {
                    MovieId = DatabaseHelperMethods.SafeInt32(reader, 0).ToString(),
                    Title = DatabaseHelperMethods.SafeStr(reader, 1),
                    //TagLine = DatabaseHelperMethods.SafeStr(reader, 2),
                    Year = DatabaseHelperMethods.SafeInt32(reader, 3),
                    Genre = DatabaseHelperMethods.SafeStr(reader, 4),
                    CoverThumbPath = DatabaseHelperMethods.SafeStr(reader, 5),
                    BackdropPath = DatabaseHelperMethods.SafeStr(reader, 6)
                };
            }
            catch (Exception ex)
            {
                Log.Error("Error reading movie information", ex);
                return null;
            }
        }

        public string GetMoviePath(string itemId, int part)
        {
            try
            {
                WebMovieDetailed movie = GetFullMovie(itemId);
                if (movie != null && movie.Files.Count >= part)
                {
                    return movie.Files[part - 1].Filename;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting movie path for " + itemId, ex);
            }
            return null;
        }
    }
}