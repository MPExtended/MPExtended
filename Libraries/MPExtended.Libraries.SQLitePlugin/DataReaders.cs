#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Data.SQLite;

namespace MPExtended.Libraries.SQLitePlugin
{
    public static class DataReaders
    {
        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadInt32(SQLiteDataReader reader, int idx)
        {
            try
            {
                object val = reader.GetValue(idx);
                if (val == null)
                    return 0;
                string sval = val.ToString();
                if (String.IsNullOrWhiteSpace(sval))
                    return 0;

                return Int32.Parse(sval, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadString(SQLiteDataReader reader, int idx)
        {
            try
            {
                object val = reader.GetValue(idx);
                if (val == null)
                    return "";

                string sval = val.ToString();
                return String.IsNullOrWhiteSpace(sval) ? String.Empty : sval;
            }
            catch (Exception)
            {
                return "";
            }
        }

        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadBoolean(SQLiteDataReader reader, int idx)
        {
            try
            {
                object val = reader.GetValue(idx);
                if (val == null)
                    return false;
                if (val is int)
                    return (int)val == 1;

                return val.ToString() == "1";
            }
            catch (Exception)
            {
                return false;
            }
        }

        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadDateTime(SQLiteDataReader reader, int idx)
        {
            try
            {
                object val = reader.GetValue(idx);
                if (val == null)
                    return new DateTime(1970, 1, 1);
                if (val is DateTime)
                    return val;

                string sval = val.ToString();
                if (String.IsNullOrWhiteSpace(sval))
                    return new DateTime(1970, 1, 1);
                return DateTime.Parse(sval);
            }
            catch (Exception)
            {
                return new DateTime(1970, 1, 1);
            }
        }

        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadFloat(SQLiteDataReader reader, int idx)
        {
            try
            {
                object val = reader.GetValue(idx);
                if (val == null)
                    return 0;
                string sval = val.ToString();
                if (String.IsNullOrEmpty(sval))
                    return 0;

                return Single.Parse(sval, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadIntAsString(SQLiteDataReader reader, int idx)
        {
            return ReadString(reader, idx);
        }

        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadStringAsInt(SQLiteDataReader reader, int idx)
        {
            return ReadInt32(reader, idx);
        }

        [AllowSQLCompare]
        [AllowSQLSort]
        public static object ReadStringAsFloat(SQLiteDataReader reader, int idx)
        {
            return ReadFloat(reader, idx);
        }

        public static object ReadList(SQLiteDataReader reader, int idx, char separator)
        {
            string txt = (string)ReadString(reader, idx);
            if (txt.Length == 0)
                return new List<string>();

            return txt.Split(separator).Select(y => y.Trim()).Where(y => y.Length > 0).Distinct().ToList();
        }

        [AllowSQLCompare('|')]
        public static object ReadPipeList(SQLiteDataReader reader, int idx)
        {
            return ReadList(reader, idx, '|');
        }

        [AllowSQLCompare("%fullsqlname LIKE '%| ' || %prepared || ' |%'")]
        [AllowSQLSort]
        public static object ReadPipeListAsString(SQLiteDataReader reader, int idx)
        {
            List<string> list = (List<string>)ReadList(reader, idx, '|');
            if (list.Count > 0)
            {
                return list.First();
            }

            return null;
        }

        public static object ReadStringAsList(SQLiteDataReader reader, int idx)
        {
            string data = (string)ReadString(reader, idx);
            if (data.Trim().Length == 0)
            {
                return new List<string>();
            }
            return new List<string>() { data };
        }
    }
}
