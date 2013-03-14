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
using System.Text;
using System.Text.RegularExpressions;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Libraries.Service.Extensions
{
    public static class NaturalSorting
    {
        // TODO: Implement support for real natural sorting, i.e. "27 Test" > "8 Test"

        private static Regex _naturalSortRegex;

        public static IOrderedEnumerable<TSource> OrderByNatural<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, WebSortOrder? order)
        {
            return OrderByNatural(source, keySelector, order.HasValue ? order.Value : WebSortOrder.Asc);
        }

        public static IOrderedEnumerable<TSource> OrderByNatural<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, WebSortOrder order)
        {
            if (_naturalSortRegex == null)
                _naturalSortRegex = new Regex(@"^\s*((a|an|the)\s+)?(.*)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (order == WebSortOrder.Asc)
                return Enumerable.OrderBy(source, x => _naturalSortRegex.Match(keySelector(x)).Groups[3].Value);
            return Enumerable.OrderByDescending(source, x => _naturalSortRegex.Match(keySelector(x)).Groups[3].Value);
        }

        public static IOrderedQueryable<TSource> OrderByNatural<TSource>(this IQueryable<TSource> source, Func<TSource, string> keySelector, WebSortOrder? order)
        {
            return OrderByNatural(source, keySelector, order.HasValue ? order.Value : WebSortOrder.Asc);
        }

        public static IOrderedQueryable<TSource> OrderByNatural<TSource>(this IQueryable<TSource> source, Func<TSource, string> keySelector, WebSortOrder order)
        {
            if (_naturalSortRegex == null)
                _naturalSortRegex = new Regex(@"^\s*((a|an|the)\s+)?(.*)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (order == WebSortOrder.Asc)
                return Queryable.OrderBy(source, x => _naturalSortRegex.Match(keySelector(x)).Groups[3].Value);
            return Queryable.OrderByDescending(source, x => _naturalSortRegex.Match(keySelector(x)).Groups[3].Value);
        }
    }
}
