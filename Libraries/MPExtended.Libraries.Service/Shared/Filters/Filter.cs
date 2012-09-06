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
            switch (Operator)
            {
                case "=":
                    return x => property.GetValue(x, null).ToString() == Value;
                case "~=":
                    return x => property.GetValue(x, null).ToString().Equals(Value, StringComparison.InvariantCultureIgnoreCase);
                case "!=":
                    return x => property.GetValue(x, null).ToString() != Value;
                case "*=":
                    return x => property.GetValue(x, null).ToString().Contains(Value, StringComparison.InvariantCultureIgnoreCase);
                case "^=":
                    return x => property.GetValue(x, null).ToString().StartsWith(Value, StringComparison.InvariantCultureIgnoreCase);
                case "$=":
                    return x => property.GetValue(x, null).ToString().EndsWith(Value, StringComparison.InvariantCultureIgnoreCase);

                // TODO: We should be able to do this *a lot* faster
                case ">":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) > Double.Parse(Value);
                case ">=":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) >= Double.Parse(Value);
                case "<":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) < Double.Parse(Value);
                case "=<":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) <= Double.Parse(Value);

                default:
                    throw new ParseException("Filter: Invalid operator '{0}'", Operator);
            }
        }
    }
}
