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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Reflection;

namespace MPExtended.Libraries.SQLitePlugin
{
    internal class AutoFiller<T> where T : new()
    {
        private IEnumerable<SQLFieldMapping> mapping;
        private Dictionary<int, SQLFieldMapping> autofillMapping;
        private Dictionary<string, PropertyInfo> autofillProperties;

        public AutoFiller(IEnumerable<SQLFieldMapping> mapping)
        {
            this.mapping = mapping;
        }

        private Dictionary<int, SQLFieldMapping> GenerateResultingMapping(SQLiteDataReader reader)
        {
            // this generates the field nr => (property name, property reader) mappings for the autofiller
            var ret = new Dictionary<int, SQLFieldMapping>();
            autofillProperties = typeof(T).GetProperties().ToDictionary(x => x.Name, x => x);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldname = reader.GetName(i);
                IEnumerable<SQLFieldMapping> matchedMappings = mapping.Where(x => x.Field == fieldname);
                if (matchedMappings.Count() == 0)
                    continue;
                SQLFieldMapping thisMapping = matchedMappings.First();
                string propertyName = thisMapping.PropertyName;
                if (!autofillProperties.ContainsKey(propertyName))
                    continue;

                ret[i] = thisMapping;
            }

            return ret;
        }

        public List<T> AutoCreateAndFill(SQLiteDataReader reader)
        {
            // automatically fill objects based upon the sql mappings provided
            if (autofillMapping == null)
                autofillMapping = GenerateResultingMapping(reader);

            // all items that we return (only one for now, maybe multiple items per row will be supported later)
            List<T> results = new List<T>() { new T() };

            // loop through all properties and get the value for it
            foreach (KeyValuePair<int, SQLFieldMapping> item in autofillMapping)
            {
                object res = item.Value.Reader.Invoke(reader, item.Key);

                // set value on all objects we return
                foreach (T obj in results)
                {
                    autofillProperties[item.Value.PropertyName].SetValue(obj, res, null);
                }
            }

            return results;
        }
    }
}
