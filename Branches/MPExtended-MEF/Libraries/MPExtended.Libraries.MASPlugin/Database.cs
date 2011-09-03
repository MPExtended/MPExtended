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

using System.Collections.Generic;
using System.Data.SQLite;

namespace MPExtended.Libraries.MASPlugin
{
    public abstract class Database
    {
        protected delegate T FillObject<T>(SQLiteDataReader reader);

        protected string DatabasePath
        {
            get;
            set;
        }

        protected Database(string databasePath)
        {
            DatabasePath = databasePath;
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder, T defaultValue, params SQLiteParameter[] parameters)
        {
            using(Query query = new Query(DatabasePath, queryString, parameters)) {
                if(query.Reader.Read()) {
                    return builder(query.Reader);
                } else {
                    return defaultValue;
                }
            }
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder, T defaultValue)
        {
            return ReadRow<T>(queryString, builder, defaultValue, new SQLiteParameter[] { });
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder, params SQLiteParameter[] parameters)
        {
            return ReadRow<T>(queryString, builder, default(T), parameters);
        }

        protected T ReadRow<T>(string queryString, FillObject<T> builder)
        {
            return ReadRow<T>(queryString, builder, default(T), new SQLiteParameter[] { });
        }

        protected List<T> ReadList<T>(string queryString, FillObject<T> builder, params SQLiteParameter[] parameters)
        {
            List<T> ret = new List<T>();

            using (Query query = new Query(DatabasePath, queryString, parameters))
            {
                while (query.Reader.Read())
                {
                    T item = builder(query.Reader);
                    if (item != null)
                        ret.Add(item);
                }
            }

            return ret;
        }

        protected List<T> ReadList<T>(string queryString, FillObject<T> builder)
        {
            return ReadList<T>(queryString, builder, new SQLiteParameter[] { });
        }
    }
}
