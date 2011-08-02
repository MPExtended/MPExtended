using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Code.Helper;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code
{
    internal class MPMusic : Database
    {
        public MPMusic()
            : base(Utils.GetMPDbLocations().Music)
        {
        }

        #region Tracks
        private List<WebMusicTrack> GetMusicTracksByWhere(string where, params SQLiteParameter[] parameters)
        {
            string sql = "SELECT idTrack, strAlbum, strArtist, strAlbumArtist, iTrack, strTitle, strPath, iDuration, iYear " +
                         "FROM tracks " +
                         (where == string.Empty ? string.Empty : "WHERE " + where);
            return ReadList<WebMusicTrack>(sql, delegate(SQLiteDataReader reader)
            {
                try
                {
                    WebMusicTrack track = new WebMusicTrack();
                    track.TrackId = DatabaseHelperMethods.SafeInt32(reader, 0);
                    track.Album = DatabaseHelperMethods.SafeStr(reader, 1);
                    track.Artists = Utils.SplitString(DatabaseHelperMethods.SafeStr(reader, 2));
                    track.AlbumArtists = Utils.SplitString(DatabaseHelperMethods.SafeStr(reader, 3));
                    track.ShortedAlbumArtist = track.AlbumArtists.FirstOrDefault();
                    track.TrackNum = DatabaseHelperMethods.SafeInt32(reader, 4);
                    track.Title = DatabaseHelperMethods.SafeStr(reader, 5);
                    track.FilePath = DatabaseHelperMethods.SafeStr(reader, 6);
                    track.Duration = DatabaseHelperMethods.SafeInt32(reader, 7);
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

        public List<WebMusicTrack> FindMusicTracks(string album, string artist, string title)
        {
            return GetMusicTracksByWhere("strAlbum LIKE @album AND strAlbumArtist LIKE @artist AND strTitle LIKE @title",
                new SQLiteParameter("@album", album + "%"),
                new SQLiteParameter("@artist", artist + "%"),
                new SQLiteParameter("@title", title + "%")
            );
        }

        public WebMusicTrack GetMusicTrack(int trackId)
        {
            return GetMusicTracksByWhere("idTrack = @id", new SQLiteParameter("@id", trackId)).FirstOrDefault();
        }

        public List<WebMusicTrack> GetSongsOfAlbum(string albumName, string albumArtist)
        {
            return GetMusicTracksByWhere("strAlbum LIKE @album and strAlbumArtist LIKE @albumArtist", 
                new SQLiteParameter("@album", albumName),
                new SQLiteParameter("@albumArtist", string.Format("%| {0} |%", albumArtist))
            );
        }

        public int GetMusicTracksCount()
        {
            return ReadInt("SELECT COUNT(*) FROM (" +
                              "SELECT idTrack " +
                              "FROM tracks" +
                           ") AS tracks");
        }

        public List<WebMusicTrack> GetAllMusicTracks()
        {
            return GetMusicTracksByWhere(string.Empty);
        }
        #endregion

        #region Artists
        public List<WebMusicArtist> GetAllArtists()
        {
            return GetArtists(null, null);
        }

        public List<WebMusicArtist> GetArtists(int? start, int? end)
        {
            // TODO: we expect only one artist in the strAlbumArtist field here. that's not correct, there can be multiple artists there
            List<WebMusicArtist> retList = ReadList<WebMusicArtist>("SELECT DISTINCT strAlbumArtist FROM tracks GROUP BY strAlbumArtist", delegate(SQLiteDataReader reader)
            {
                if (!String.IsNullOrEmpty(Utils.ClearString(DatabaseHelperMethods.SafeStr(reader, 0))))
                { return new WebMusicArtist(Utils.ClearString(DatabaseHelperMethods.SafeStr(reader, 0))); }
                else
                { return new WebMusicArtist("No Title"); }
            }, start, end);
            return retList;
        }

        public int GetArtistsCount()
        {
            return ReadInt("SELECT COUNT(*) FROM (SELECT DISTINCT strAlbumArtist FROM tracks) AS tracks");
        }
        #endregion

        #region Albums
        private List<WebMusicAlbum> GetAlbumsByWhere(string where, int? start, int? end, params SQLiteParameter[] parameters)
        {
            string sql = "SELECT t.strAlbum, t.strAlbumArtist, t.strArtist, a.iYear, g.strGenre " +
                         "FROM tracks t " +
                         "LEFT JOIN albuminfo a ON t.strAlbum = a.strAlbum " + // this table is empty for me
                         "LEFT JOIN genre g ON a.idGenre = g.strGenre " +
                         "WHERE t.strAlbum != '' " + (where == string.Empty ? "" : " AND " + where);
            Dictionary<string, WebMusicAlbum> ret = new Dictionary<string, WebMusicAlbum>();
            ReadList<WebMusicAlbum>(sql, delegate(SQLiteDataReader reader) {
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
                        ret[title] = new WebMusicAlbum() {
                            Title = title,
                            AlbumArtists = albumartists,
                            Artists = artists,
                            Year = (uint)DatabaseHelperMethods.SafeInt32(reader, 3),
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

            return ret.Select(kp => new WebMusicAlbum() { 
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
            return ReadInt("SELECT COUNT(*) FROM (" +
                             "SELECT DISTINCT t.strAlbum " +
                             "FROM tracks t " +
                             "WHERE t.strAlbum != ''" +
                           ") AS albums");
        }

        public List<WebMusicAlbum> GetAlbums(int? start, int? end)
        {
            return GetAlbumsByWhere(string.Empty, start, end);
        }

        public List<WebMusicAlbum> GetAlbumsByArtist(string artistName)
        {
            return GetAlbumsByWhere("t.strAlbumArtist LIKE @albumArtist", null, null, new SQLiteParameter("@albumArtist", String.Format("%| {0} |%", artistName)));
        }

        public WebMusicAlbum GetAlbum(string albumName, string artistName)
        {
            return GetAlbumsByWhere("t.strAlbumArtist LIKE @albumArtist AND t.strAlbum LIKE @album", null, null,
                new SQLiteParameter("@albumArtist", String.Format("%| {0} |%", artistName)),
                new SQLiteParameter("@album", albumName)
            ).FirstOrDefault();
        }

        public List<WebMusicAlbum> GetAllAlbums()
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
                WebMusicTrack track = GetMusicTrack(Int32.Parse(itemId));
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
    }
}