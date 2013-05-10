#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.IO;

namespace MPExtended.Libraries.SQLitePlugin
{
    public class DatabaseConnection : IDisposable
    {
        private bool shouldClose;
        private FileStream fileStream;

        public SQLiteConnection Connection { get; private set; }
        public string Path { get; private set; }

        public bool IsOpen
        {
            get
            {
                return Connection != null && Connection.State != ConnectionState.Broken && Connection.State != ConnectionState.Closed;
            }
        }

        public DatabaseConnection(Database db, bool holdExclusiveLock)
        {
            string connectionString = "Data Source=" + db.DatabasePath + ";Read Only=True";
            Connection = new SQLiteConnection(connectionString);
            Path = db.DatabasePath;
            if (holdExclusiveLock)
                fileStream = File.Open(db.DatabasePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            Connection.Open();
            shouldClose = true;
        }

        public DatabaseConnection(SQLiteConnection connection, bool mayClose)
        {
            Connection = connection;
            shouldClose = mayClose;
        }

        public Query CreateQuery(string query)
        {
            return new Query(this, query);
        }

        public Query CreateQuery(string query, params SQLiteParameter[] parameters)
        {
            return new Query(this, query, parameters);
        }

        public void Dispose()
        {
            if (IsOpen && shouldClose)
            {
                try
                {
                    Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }
            }
        }
    }
}
