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
using System.Data.SQLite;

namespace MPExtended.Libraries.SQLitePlugin
{
    public class Exec : IDisposable
    {
        private DatabaseConnection db;
        private SQLiteCommand cmd;

        public Exec(DatabaseConnection database, string query)
        {
            db = database;
            cmd = db.Connection.CreateCommand();
            cmd.CommandText = query;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                throw new QueryException("Failed to execute command", query, db.Path, ex);
            }
        }

        public void Dispose()
        {
            cmd.Dispose();
        }
    }
}
