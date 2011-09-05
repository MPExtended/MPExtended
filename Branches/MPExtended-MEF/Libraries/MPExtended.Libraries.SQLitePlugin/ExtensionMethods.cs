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
using System.Text;
using System.Data.SQLite;

namespace MPExtended.Libraries.SQLitePlugin
{
    public static class SQLiteDataReaderExtensionMethods
    {
        public static int ReadInt32(this SQLiteDataReader reader, int idx)
        {
            return (int)DataReaders.ReadInt32(reader, idx);
        }

        public static string ReadString(this SQLiteDataReader reader, int idx)
        {
            return (string)DataReaders.ReadString(reader, idx);
        }

        public static bool ReadBoolean(this SQLiteDataReader reader, int idx)
        {
            return (bool)DataReaders.ReadBoolean(reader, idx);
        }

        public static DateTime ReadDateTime(this SQLiteDataReader reader, int idx)
        {
            return (DateTime)DataReaders.ReadDateTime(reader, idx);
        }

        public static float ReadFloat(this SQLiteDataReader reader, int idx)
        {
            return (float)DataReaders.ReadFloat(reader, idx);
        }

        public static IList<string> ReadList(this SQLiteDataReader reader, int idx, char separator)
        {
            return (IList<string>)DataReaders.ReadList(reader, idx, separator);
        }

        public static IList<string> ReadPipeList(this SQLiteDataReader reader, int idx)
        {
            return (IList<string>)DataReaders.ReadPipeList(reader, idx);
        }

        public static string ReadIntAsString(this SQLiteDataReader reader, int idx)
        {
            return (string)DataReaders.ReadIntAsString(reader, idx);
        }
    }
}
