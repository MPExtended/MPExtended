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
using System.Linq;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal static class Filters
    {
        public static List<WebFilterOperator> Operators { get; private set; }

        static Filters()
        {
            Operators = new List<WebFilterOperator> {
                new WebFilterOperator() { Operator = "==", Title = "equals", SuitableTypes = new List<string>() { "string", "integer", "boolean" } },
                new WebFilterOperator() { Operator = "!=", Title = "not", SuitableTypes = new List<string>() { "string", "integer", "boolean" } },
                new WebFilterOperator() { Operator = ">",  Title = "greater than", SuitableTypes = new List<string>() { "string", "integer" } },
                new WebFilterOperator() { Operator = "<",  Title = "less than", SuitableTypes = new List<string>() { "string", "integer" } },
                new WebFilterOperator() { Operator = ">=", Title = "greater or equal than", SuitableTypes = new List<string>() { "string", "integer" } },
                new WebFilterOperator() { Operator = "<=", Title = "less or equal than", SuitableTypes = new List<string>() { "string", "integer" } },

                new WebFilterOperator() { Operator = "*=",  Title = "contains", SuitableTypes = new List<string>() { "list", "string" } },
                new WebFilterOperator() { Operator = "^=",  Title = "starts with", SuitableTypes = new List<string>() { "string" } },
                new WebFilterOperator() { Operator = "$=",  Title = "ends with", SuitableTypes = new List<string>() { "string" } }
            };
        }

        public static IEnumerable<string> GetAllValues<T>(string field, IEnumerable<T> items)
        {
            var type = items.GetType().GetGenericArguments().First();
            var property = type.GetProperty(field);
            return items
                .Select(x => property.GetValue(x, null).ToString())
                .Distinct()
                .OrderBy(x => x);
        }
    }

    internal class Filter
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }

    internal class FilterList : List<Filter>
    {
        private static Regex parseFilterRegex = new Regex(@"^\s*([A-Za-z]*)\s*([=!><*$\^]+)\s*(.*?)\s*$", RegexOptions.Compiled);

        private delegate bool MatchDelegate(object x);

        public FilterList(string filterText)
        {
            string[] segments = filterText.Contains(',') ? filterText.Split(',') : new string[] { filterText };
            foreach (var segment in segments)
            {
                var match = parseFilterRegex.Match(segment);
                Add(new Filter()
                {
                    Field = match.Groups[1].Value,
                    Operator = match.Groups[2].Value,
                    Value = match.Groups[3].Value
                });
            }
        }

        public FilterList(params Filter[] filters)
        {
            foreach (var filter in filters)
                Add(filter);
        }

        public IEnumerable<T> MatchList<T>(IEnumerable<T> items)
        {
            var type = items.GetType().GetGenericArguments().First();
            foreach (var filter in this)
            {
                var f = filter; // Avoid closing over the loop variable, which isn't possible until C# 5 (see: http://blogs.msdn.com/b/ericlippert/archive/2009/11/12/closing-over-the-loop-variable-considered-harmful.aspx)
                var property = type.GetProperty(filter.Field);
                items = items.Where(x => GetMatchDelegate(property, f.Operator, f.Value).Invoke(x));
            }
            return items;
        }

        public bool Matches<T>(T item)
        {
            foreach (var filter in this)
            {
                var property = item.GetType().GetProperty(filter.Field);
                if (!GetMatchDelegate(property, filter.Operator, filter.Value).Invoke(item))
                    return false;
            }

            return true;
        }

        private MatchDelegate GetMatchDelegate(PropertyInfo property, string op, string value)
        {
            switch (op)
            {
                case "=":
                    return x => property.GetValue(x, null).ToString() == value;
                case "!=":
                    return x => property.GetValue(x, null).ToString() != value;
                case ">":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) > Double.Parse(value);
                case ">=":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) >= Double.Parse(value);
                case "<":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) < Double.Parse(value);
                case "=<":
                    return x => Double.Parse(property.GetValue(x, null).ToString(), CultureInfo.InvariantCulture) <= Double.Parse(value);
                case "*=":
                    return x => property.GetValue(x, null).ToString().Contains(value);
                case "^=":
                    return x => property.GetValue(x, null).ToString().StartsWith(value);
                case "$=":
                    return x => property.GetValue(x, null).ToString().EndsWith(value);
                default:
                    throw new ArgumentException("Invalid operator");
            }
        }

        public override string ToString()
        {
            return String.Join(",", this.Select(f => f.Field + f.Operator + f.Value));
        }
    }
}
