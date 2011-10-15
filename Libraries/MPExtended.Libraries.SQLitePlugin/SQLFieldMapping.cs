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
using System.Data.SQLite;
using System.Text;

namespace MPExtended.Libraries.SQLitePlugin
{
    public class SQLFieldMapping
    {
        public delegate object ReadValue(SQLiteDataReader reader, int index);

        public string Table { get; set; }
        public string Field { get; set; }
        public string PropertyName { get; set; }
        public ReadValue Reader { get; set; }

        public string FullSQLName
        {
            get
            {
                return this.Table.Length > 0 ? this.Table + "." + this.Field : this.Field;
            }
        }

        public SQLFieldMapping()
        {
        }

        public SQLFieldMapping(string field, string propertyname, ReadValue reader)
        {
            this.Table = string.Empty;
            this.Field = field;
            this.PropertyName = propertyname;
            this.Reader = reader;
        }

        public SQLFieldMapping(string table, string field, string propertyname, ReadValue reader)
        {
            this.Table = table;
            this.Field = field;
            this.PropertyName = propertyname;
            this.Reader = reader;
        }
    }
}
