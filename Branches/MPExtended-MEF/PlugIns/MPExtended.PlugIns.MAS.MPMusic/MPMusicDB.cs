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
using MPExtended.Services.MediaAccessService.Interfaces.Music;

namespace MPExtended.PlugIns.MAS.MPMusic
{
    internal class MPMusicDB : Database
    {
        public MPMusicDB()
            : base(Configuration.GetMPDbLocations().Music)
        {
        }

        #region Tracks
        private List<WebMusicTrackBasic> GetMusicTracksByWhere(string where, params SQLiteParameter[] parameters)
        {
            string sql = "SELECT idTrack, strAlbum, strArtist, strAlbumArtist, iTrack, strTitle, strPath, iDuration, iYear " +
                         "FROM tracks " +
                         (where == string.Empty ? string.Empty : "WHERE " + where);
            return ReadList<WebMusicTrackBasic>(sql, delegate(SQLiteDataReader reader)
            {
                try
                {
                    WebMusicTrackBasic track = new WebMusicTrackBasic();
                    track.TrackId = DatabaseHelperMethods.SafeInt32(reader, 0).ToString();
                    //track.Album = DatabaseHelperMethods.SafeStr(reader, 1);
                    //track.Artists = Utils.SplitString(DatabaseHelperMethods.SafeStr(reader, 2));
                    //track.AlbumArtists = Utils.SplitString(DatabaseHelperMethods.SafeStr(reader, 3));
                    //track.ShortedAlbumArtist = track.AlbumArtists.FirstOrDefault();
                    track.TrackNumber = DatabaseHelperMethods.SafeInt32(reader, 4);
                    track.Title = DatabaseHelperMethods.SafeStr(reader, 5);
                    track.FilePath = DatabaseHelperMethods.SafeStr(reader, 6);
                    //track.Duration = DatabaseHelperMethods.SafeInt32(reader, 7);
                    track.Year = DatabaseHelperMethods.SafeInt32(reader, 8);
                    return track;
                }
                catch (Exception ex)
                {
                    Log.Error("Error reading music track", ex);
                    return null;
                }
            }, null, null, parameters);
        }

        public List<WebMusicTrackBasic> FindMusicTracks(string album, string artist, string title)
        {
            return GetMusicTracksByWhere("strAlbum LIKE @album AND strAlbumArtist LIKE @artist AND strTitle LIKE @title",
                new SQLiteParameter("@album", album + "%"),
                new SQLiteParameter("@artist", artist + "%"),
                new SQLiteParameter("@title", title + "%")
            );
        }

        public WebMusicTrackBasic GetMusicTrack(int trackId)
        {
            return GetMusicTracksByWhere("idTrack = @id", new SQLiteParameter("@id", trackId)).FirstOrDefault();
        }

        public List<WebMusicTrackBasic> GetSongsOfAlbum(string albumName, string albumArtist)
        {
            return GetMusicTracksByWhere("strAlbum LIKE @album and strAlbumArtist LIKE @albumArtist", 
                new SQLiteParameter("@album", albumName),
                new SQLiteParameter("@albumArtist", string.Format("%| {0} |%", albumArtist))
            );
        }

        public int GetMusicTracksCount()
        {
            return GetAllMusicTracks().Count;
        }

        public List<WebMusicTrackBasic> GetAllMusicTracks()
        {
            return GetMusicTracksByWhere(string.Empty);
        }
        #endregion

        #region Artists
        public List<WebMusicArtistBasic> GetAllArtists()
        {
            return GetArtists(null, null);
        }

        public List<WebMusicArtistBasic> GetArtists(int? start, int? end)
        {
            // TODO: we expect only one artist in the strAlbumArtist field here. that's not correct, there can be multiple artists there
            List<WebMusicArtistBasic> retList = ReadList<WebMusicArtistBasic>("SELECT DISTINCT strAlbumArtist FROM tracks GROUP BY strAlbumArtist", delegate(SQLiteDataReader reader)
            {
                if (!String.IsNullOrEmpty(ClearString(DatabaseHelperMethods.SafeStr(reader, 0))))
                {
                    return new WebMusicArtistBasic() 
                    {
                        Title = ClearString(DatabaseHelperMethods.SafeStr(reader, 0)),
                        ArtistId = ClearString(DatabaseHelperMethods.SafeStr(reader, 0))
                    
                    };
                }
                else
                {
                    return new WebMusicArtistBasic()
                    {
                        Title = "No title",
                        ArtistId = ""

                    }; 
                }
            }, start, end);
            return retList;
        }

        #endregion

        #region Albums
        private List<WebMusicAlbumBasic> GetAlbumsByWhere(string where, int? start, int? end, params SQLiteParameter[] parameters)
        {
            string sql = "SELECT t.strAlbum, t.strAlbumArtist, t.strArtist, a.iYear, g.strGenre " +
                         "FROM tracks t " +
                         "LEFT JOIN albuminfo a ON t.strAlbum = a.strAlbum " + // this table is empty for me
                         "LEFT JOIN genre g ON a.idGenre = g.strGenre " +
                         "WHERE t.strAlbum != '' " + (where == string.Empty ? "" : " AND " + where);
            var ret = new Dictionary<string, WebMusicAlbumBasic>();
            ReadList<WebMusicAlbumBasic>(sql, delegate(SQLiteDataReader reader) {
                try {
                    string title = DatabaseHelperMethods.SafeStr(reader, 0);
                    string[] albumartists = Utils.SplitString(DatabaseHelperMethods.SafeStr(reader, 1));
                    string[] artists = Utils.SplitString(DatabaseHelperMethods.SafeStr(reader, 2));
                    if (ret.ContainsKey(title))
                    {
                        ret[title].AlbumArtists = ret[title].AlbumArtists.Concat(albumartists).ToArray();
                    }
                    else
                    {
                        ret[title] = new WebMusicAlbumBasic() {
                            Title = title,
                            AlbumArtists = albumartists,
                            Artists = artists,
                            Year = DatabaseHelperMethods.SafeInt32(reader, 3),
                            Genre = reader.IsDBNull(4) ? null : DatabaseHelperMethods.SafeStr(reader, 4)
                        };
                    }

                } 
                catch(Exception ex) 
                {
                    Log.Error("Error reading music album", ex);
                }

                return null;
            }, start, end, parameters);

            return ret.Select(kp => new WebMusicAlbumBasic() { 
                AlbumArtists = kp.Value.AlbumArtists.Distinct().ToArray(),
                Artists = kp.Value.Artists.Distinct().ToArray(),
                CoverPathL = GetLargeAlbumCover(kp.Value.AlbumArtists.Distinct().First(), kp.Value.Title),
                Genre = kp.Value.Genre,
                ShortedAlbumArtist = kp.Value.AlbumArtists.First(),
                Title = kp.Value.Title,
                Year = kp.Value.Year
            }).ToList();
        }

        public int GetAlbumsCount()
        {
            return GetAllAlbums().Count;
        }

        public List<WebMusicAlbumBasic> GetAlbums(int? start, int? end)
        {
            return GetAlbumsByWhere(string.Empty, start, end);
        }

        public List<WebMusicAlbumBasic> GetAlbumsByArtist(string artistName)
        {
            return GetAlbumsByWhere("t.strAlbumArtist LIKE @albumArtist", null, null, new SQLiteParameter("@albumArtist", String.Format("%| {0} |%", artistName)));
        }

        public WebMusicAlbumBasic GetAlbum(string albumName, string artistName)
        {
            return GetAlbumsByWhere("t.strAlbumArtist LIKE @albumArtist AND t.strAlbum LIKE @album", null, null,
                new SQLiteParameter("@albumArtist", String.Format("%| {0} |%", artistName)),
                new SQLiteParameter("@album", albumName)
            ).FirstOrDefault();
        }

        public List<WebMusicAlbumBasic> GetAllAlbums()
        {
            return GetAlbumsByWhere(string.Empty, null, null);
        }
        #endregion

        /// <summary>
        /// Gets the path to a media item
        /// </summary>
        /// <param name="itemId">Id of the media item</param>
        /// <returns>Path to the mediaitem or null if item id doesn't exist</returns>
        internal String GetTrackPath(string itemId)
        {
            try
            {
                WebMusicTrackBasic track = GetMusicTrack(Int32.Parse(itemId));
                if (track != null)
                {
                    return track.FilePath;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting music path for " + itemId, ex);
            }
            return null;
        }

        private string GetLargeAlbumCover(string artistName, string albumName)
        {
            if (String.IsNullOrEmpty(artistName) || String.IsNullOrEmpty(albumName))
                return string.Empty;

            artistName = artistName.Trim(new char[] { '|', ' ' });
            albumName = albumName.Replace(":", "_");

            return System.IO.Path.Combine(Utils.GetBannerPath("music"), "Albums", artistName + "-" + albumName + "L.jpg");
        }

        public static string ClearString(String _stringToClean)
        {
            return _stringToClean.Substring(2, (_stringToClean.Length - 5));
        }
    }
}