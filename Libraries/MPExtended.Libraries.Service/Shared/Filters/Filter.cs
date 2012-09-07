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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Shared.Filters
{
    internal class Filter : IFilter
    {
        public string Field { get; private set; }
        public string Operator { get; private set; }
        public string Value { get; private set; }

        private delegate bool MatchDelegate(object x);

        private Type expectedType;
        private PropertyInfo property;
        private MatchDelegate matcher;

        private int intValue;
        private long longValue;

        public Filter(string field, string oper, string value)
        {
            Field = field;
            Operator = oper;
            Value = value;
        }

        public void ExpectType(Type type)
        {
            expectedType = type;
            property = expectedType.GetProperty(Field);
            matcher = GetMatchDelegate();
        }

        public bool Matches<T>(T obj)
        {
            return matcher(obj);
        }

        private MatchDelegate GetMatchDelegate()
        {
            if (property.PropertyType == typeof(string))
                return GetStringMatchDelegate();
            if (property.PropertyType == typeof(int))
                return GetIntMatchDelegate();
            if (property.PropertyType == typeof(long))
                return GetLongMatchDelegate();
            if (property.PropertyType.GetInterfaces().Any(x => x == typeof(IEnumerable)))
                return GetListMatchDelegate();

            Log.Error("Filter: Cannot load match delegate for field of type '{0}' (property name {1})", property.PropertyType, property.Name);
            throw new ArgumentException("Filter: Cannot filter on field of type '{0}'", property.PropertyType.ToString());
        }

        private MatchDelegate GetStringMatchDelegate()
        {
            switch (Operator)
            {
                case "=":
                    return x => (string)property.GetValue(x, null) == Value;
                case "~=":
                    return x => ((string)property.GetValue(x, null)).Equals(Value, StringComparison.InvariantCultureIgnoreCase);
                case "!=":
                    return x => (string)property.GetValue(x, null) != Value;
                case "*=":
                    return x => ((string)property.GetValue(x, null)).Contains(Value, StringComparison.InvariantCultureIgnoreCase);
                case "^=":
                    return x => ((string)property.GetValue(x, null)).StartsWith(Value, StringComparison.InvariantCultureIgnoreCase);
                case "$=":
                    return x => ((string)property.GetValue(x, null)).EndsWith(Value, StringComparison.InvariantCultureIgnoreCase);

                default:
                    throw new ParseException("Filter: Invalid operator '{0}' for string field", Operator);
            }
        }

        private MatchDelegate GetIntMatchDelegate()
        {
            if (!Int32.TryParse(Value, out intValue))
                throw new ArgumentException("Filter: Invalid value '{0}' for integer field", Value);

            switch (Operator)
            {
                case "=":
                    return x => (int)property.GetValue(x, null) == intValue;
                case "!=":
                    return x => (int)property.GetValue(x, null) != intValue;
                case ">":
                    return x => (int)property.GetValue(x, null) > intValue;
                case ">=":
                    return x => (int)property.GetValue(x, null) >= intValue;
                case "<":
                    return x => (int)property.GetValue(x, null) < intValue;
                case "<=":
                    return x => (int)property.GetValue(x, null) <= intValue;
                default:
                    throw new ArgumentException("Filter: Invalid operator '{0}' for integer field", Operator);
            }
        }

        private MatchDelegate GetLongMatchDelegate()
        {
            if (!Int64.TryParse(Value, out longValue))
                throw new ArgumentException("Filter: Invalud value '{0}' for integer field", Value);

            switch (Operator)
            {
                case "=":
                    return x => (long)property.GetValue(x, null) == longValue;
                case "!=":
                    return x => (long)property.GetValue(x, null) != longValue;
                case ">":
                    return x => (long)property.GetValue(x, null) > longValue;
                case ">=":
                    return x => (long)property.GetValue(x, null) >= longValue;
                case "<":
                    return x => (long)property.GetValue(x, null) < longValue;
                case "<=":
                    return x => (long)property.GetValue(x, null) <= longValue;
                default:
                    throw new ArgumentException("Filter: Invalid operator '{0}' for integer field", Operator);
            }
        }

        private MatchDelegate GetListMatchDelegate()
        {
            switch (Operator)
            {
                case "*=":
                    return delegate(object x)
                        {
                            foreach(var item in (IEnumerable)property.GetValue(x, null))
                            {
                                if (item.ToString() == Value)
                                    return true;
                            }

                            return false;
                        };
                default:
                    throw new ArgumentException("Filter: Invalid operator '{0}' for list field", Operator);
            }
        }
    }
}
