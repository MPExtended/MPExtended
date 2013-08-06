#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
    public class Query : IDisposable
    {
        private DatabaseConnection db;
        private SQLiteCommand cmd;

        public SQLiteDataReader Reader
        {
            get;
            private set;
        }

        public Query(DatabaseConnection database, string query, params SQLiteParameter[] parameters)
        {
            db = database;
            cmd = db.Connection.CreateCommand();
            cmd.CommandText = query;
            foreach (SQLiteParameter param in parameters)
                cmd.Parameters.Add(param);

            try
            {
                Reader = cmd.ExecuteReader();
            }
            catch (SQLiteException ex)
            {
                throw new QueryException("Failed to execute query", query, db.Path, ex);
            }
        }

        public Query(DatabaseConnection database, string query)
            : this(database, query, new SQLiteParameter[] { })
        {
        }

        public void Dispose()
        {
            Reader.Close();
            Reader.Dispose();  
            cmd.Dispose();
        }
    }
}
