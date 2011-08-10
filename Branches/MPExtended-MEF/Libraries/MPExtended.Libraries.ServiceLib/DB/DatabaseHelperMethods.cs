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
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Libraries.ServiceLib.DB
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
                catch (Exception)
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
                catch (Exception)
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