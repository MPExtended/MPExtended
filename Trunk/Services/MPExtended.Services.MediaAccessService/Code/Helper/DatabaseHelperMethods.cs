using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Services.MediaAccessService.Code.Helper
{
   
        public class DatabaseHelperMethods
        {

            public static String SQLiteConnStr(string dbName)
            {
                return "Data Source=" + dbName + ";Read Only=True";
            }

            #region Utils


            public static string SafeStr(SQLiteDataReader reader, int idx)
            {
                try
                {
                    if (reader.IsDBNull(idx))
                        return "";
                    else
                        return reader.GetString(idx);
                }
                catch (System.InvalidCastException)
                {
                    //TODO: This is a workaround and should be properly handled. It fixes the problem that numeric values
                    //(e.g. seriesname==24) will throw an invalidcastexception)
                    //see http://forum.team-mediaportal.com/webservice-mobile-access-537/webservice-general-media-access-webservice-89956/index7.html#post748640)
                    int intString = SafeInt32(reader, idx);
                    return intString.ToString();
                }
                catch (Exception ex)
                {
                    return "";
                }
            }


            public static Int32 SafeInt32(SQLiteDataReader reader, int idx)
            {
                try
                {
                    if (reader.IsDBNull(idx))
                        return 0;
                    else
                        return reader.GetInt32(idx);
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            public static bool SafeBoolean(SQLiteDataReader reader, int idx)
            {
                try
                {
                    if (reader.IsDBNull(idx))
                        return false;
                    else
                        return reader.GetBoolean(idx);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static DateTime SafeDateTime(SQLiteDataReader reader, int idx)
            {
                try
                {
                    if (reader.IsDBNull(idx))
                        return new DateTime(1970, 1, 1);
                    else
                    {
                        String s = reader.GetString(idx);
                        return DateTime.Parse(s);
                    }
                }
                catch (Exception ex)
                {
                    return new DateTime(1970, 1, 1);
                }
            }

            public static float SafeFloat(SQLiteDataReader reader, int idx)
            {
                try
                {
                    if (reader.IsDBNull(idx))
                        return 0;
                    else
                        return reader.GetFloat(idx);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            #endregion
        }
   
}