#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.TVAccessService
{
    internal static class IEnumerableExtensionMethods
    {
        public static IEnumerable<T> TakeRange<T>(this IEnumerable<T> source, int start, int end)
        {
            int count = end - start + 1;

            if (source is List<T>)
                return ((List<T>)source).GetRange(start, count);
            return source.Skip(start).Take(count);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, OrderBy? order)
        {
            if (order == MPExtended.Services.TVAccessService.Interfaces.OrderBy.Desc)
                return Enumerable.OrderByDescending(source, keySelector);
            return Enumerable.OrderBy(source, keySelector);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, OrderBy? order)
        {
            if (order == MPExtended.Services.TVAccessService.Interfaces.OrderBy.Desc)
                return Enumerable.ThenByDescending(source, keySelector);
            return Enumerable.ThenBy(source, keySelector);
        }

        public static IOrderedEnumerable<T> SortChannelList<T>(this IEnumerable<T> list, SortBy? sortInput, OrderBy? orderInput) where T : WebChannelBasic
        {
            switch (sortInput)
            {
                case SortBy.Name:
                default:
                    return list.OrderBy(x => x.DisplayName, orderInput);
            }
        }

        public static IOrderedEnumerable<T> SortGroupList<T>(this IEnumerable<T> list, SortBy? sortInput, OrderBy? orderInput) where T : WebChannelGroup
        {
            switch (sortInput)
            {
                case SortBy.Name:
                    return list.OrderBy(x => x.GroupName, orderInput);
                case SortBy.User:
                default:
                    return list.OrderBy(x => x.SortOrder, orderInput);
            }
        }

        public static IOrderedEnumerable<T> SortScheduleList<T>(this IEnumerable<T> list, SortBy? sortInput, OrderBy? orderInput) where T : WebScheduleBasic
        {
            switch (sortInput)
            {
                case SortBy.Name:
                default:
                    return list.OrderBy(x => x.ProgramName, orderInput);
                case SortBy.Channel:
                    return list.OrderBy(x => x.IdChannel, orderInput);
            }
        }

        public static IOrderedEnumerable<T> SortRecordingList<T>(this IEnumerable<T> list, SortBy? sortInput, OrderBy? orderInput) where T : WebRecordingBasic
        {
            switch (sortInput)
            {
                case SortBy.Name:
                default:
                    return list.OrderBy(x => x.Title, orderInput);
                case SortBy.Channel:
                    return list.OrderBy(x => x.IdChannel, orderInput);
            }
        }
    }
}
