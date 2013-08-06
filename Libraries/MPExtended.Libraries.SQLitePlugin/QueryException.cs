#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://mpextended.github.com/
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
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace MPExtended.Libraries.SQLitePlugin
{
    [Serializable]
    public class QueryException : Exception
    {
        public string Query { get; private set; }
        public string Database { get; private set; }

        public QueryException()
            : base()
        {
        }

        public QueryException(string message)
            : base(message)
        {
        }

        public QueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public QueryException(string message, string query, Exception innerException)
            : base (message, innerException)
        {
            Query = query;
        }

        public QueryException(string message, string query, string database, Exception innerException)
            : base (message, innerException)
        {
            Query = query;
            Database = database;
        }

        public override string ToString()
        {
            var description = new StringBuilder();
            description.AppendFormat("{0}: {1}", this.GetType(), Message);

            if (Query != null)
                description.AppendFormat("{0}Query: {1}", Environment.NewLine, Query);
            if (Database != null)
                description.AppendFormat("{0}Database: {1}", Environment.NewLine, Database);
            if (InnerException != null)
                description.AppendFormat(" ---> {0}{1}   --- End of inner exception stack trace ---{1}", InnerException, Environment.NewLine);
            description.Append(StackTrace);

            return description.ToString();
        }
    }
}
