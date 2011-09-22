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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;
using System.Data.SQLite;
using MPExtended.Libraries.SQLitePlugin;

namespace MPExtended.PlugIns.MAS.MPMusic
{
    [Export(typeof(IMusicLibrary))]
    [ExportMetadata("Database", "MPMyMusic")]
    [ExportMetadata("Version", "1.0.0.0")]
    public class MPMusic :  IMusicLibrary
    {
        private IPluginData data;

        [ImportingConstructor]
        public MPMusic(IPluginData data) 
        {
            this.data = data;
        }

        public IEnumerable<WebMusicTrackBasic> GetAllTracks()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebMusicAlbumBasic> GetAllAlbums()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebMusicArtistBasic> GetAllArtists()
        {
            //string sql = "SELECT DISTINCT strAlbumArtist FROM tracks GROUP BY strAlbumArtist";
            //return new LazyQuery<WebMusicArtistBasic>(this, sql, new List<SQLFieldMapping>() {
            //    new SQLFieldMapping("", "strAlbumArtist", "Id", DataReaders.ReadString),
            //    new SQLFieldMapping("", "strAlbumArtist", "Title", DataReaders.ReadString)
       
            //});
            throw new NotImplementedException();
        }

        public IEnumerable<WebMusicTrackDetailed> GetAllTracksDetailed()
        {
            throw new NotImplementedException();
        }

        public WebMusicTrackBasic GetTrackBasicById(string trackId)
        {
            throw new NotImplementedException();
        }

        public WebMusicAlbumBasic GetAlbumBasicById(string albumId)
        {
            throw new NotImplementedException();
        }

        public WebMusicArtistBasic GetArtistBasicById(string artistId)
        {
            throw new NotImplementedException();
        }

        public WebMusicTrackDetailed GetTrackDetailedById(string trackId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WebGenre> GetAllGenres()
        {
            //string sql = "SELECT DISTINCT strGenre FROM tracks";
            //return ReadList<IEnumerable<string>>(sql, delegate(SQLiteDataReader reader)
            //{
            //    return reader.ReadPipeList(0);
            //})
            //        .SelectMany(x => x)
            //        .Distinct()
            //        .OrderBy(x => x)
            //        .Select(x => new WebGenre() { Name = x.Replace("| ", "").Replace(" |", "") });
            throw new NotImplementedException();
        }

        public IEnumerable<WebCategory> GetAllCategories()
        {
            throw new NotImplementedException();
        }

        public DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }


        public Stream GetCover(string albumId, int offset)
        {
            throw new NotImplementedException();
        }

        public Stream GetBackdrop(string albumId, int offset)
        {
            throw new NotImplementedException();
        }

        public bool IsLocalFile(string path)
        {
            throw new NotImplementedException();
        }

        public Stream GetFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
