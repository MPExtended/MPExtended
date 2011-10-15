#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Text;
using MPExtended.Libraries.SQLitePlugin;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using System.Data.SQLite;

#if false
namespace MPExtended.PlugIns.MAS.XBMCMusic
{
    internal class XBMCMusicDB : Database
    {
        public XBMCMusicDB()
            : base(@"C:\Users\T.Lindener\AppData\Roaming\XBMC\userdata\Database\MyMusic16.db")
        {
        }
        public List<WebMusicArtistBasic> GetArtists(int? start, int? end)
        {
            // TODO: we expect only one artist in the strAlbumArtist field here. that's not correct, there can be multiple artists there
            List<WebMusicArtistBasic> retList = ReadList<WebMusicArtistBasic>("SELECT idArtist,strArtist FROM artist", delegate(SQLiteDataReader reader)
            {
                if (!String.IsNullOrEmpty(DatabaseHelperMethods.SafeStr(reader, 1)))
                {
                    return new WebMusicArtistBasic()
                    {
                        Title = DatabaseHelperMethods.SafeStr(reader, 1),
                        Id = DatabaseHelperMethods.SafeStr(reader, 0)

                    };
                }
                else
                {
                    return null;
                }
            });
            return retList.GetRange(start.Value, end.Value).Where(p => p != null).ToList();
        }
    }
}
#endif