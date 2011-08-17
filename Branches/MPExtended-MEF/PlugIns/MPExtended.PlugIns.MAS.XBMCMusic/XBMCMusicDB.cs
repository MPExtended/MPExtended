using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Libraries.ServiceLib.DB;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using System.Data.SQLite;
using MPExtended.Libraries.ServiceLib;

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
            }, start, end);
            return retList.Where(p => p != null).ToList();
        }
    }
}
