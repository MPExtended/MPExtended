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
using System.Linq.Expressions;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService
{
    internal static class IEnumerableExtensionMethods
    {
        // Missing in IList for some reason, but let's just implement it for all IEnumerables
        public static IEnumerable<T> GetRange<T>(this IEnumerable<T> source, int index, int count)
        {
            if (source is List<T>)
                return ((List<T>)source).GetRange(index, count);
            if (source is ILazyQuery<T>)
                return ((ILazyQuery<T>)source).GetRange(index, count);
            return source.Skip(index).Take(count);
        }

        // Take advantage of lazy queries
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Expression<Func<T, bool>> predicate)
        {
            if (source is ILazyQuery<T>)
                return ((ILazyQuery<T>)source).Where(predicate);
            return Enumerable.Where(source, predicate.Compile());
        }

        // Take advantage of lazy queries
        public static int Count<T>(this IEnumerable<T> source)
        {
            if (source is ILazyQuery<T>)
                return ((ILazyQuery<T>)source).Count();
            return Enumerable.Count(source);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Expression<Func<TSource, TKey>> keySelector, OrderBy order)
        {
            if (source is ILazyQuery<TSource>)
            {
                ILazyQuery<TSource> lazy = (ILazyQuery<TSource>)source;
                if (order == MPExtended.Services.MediaAccessService.Interfaces.OrderBy.Asc)
                    return lazy.OrderBy(keySelector);
                return lazy.OrderByDescending(keySelector);
            }

            var comp = keySelector.Compile();
            if (order == MPExtended.Services.MediaAccessService.Interfaces.OrderBy.Asc)
                return Enumerable.OrderBy(source, comp);
            return Enumerable.OrderByDescending(source, comp);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Expression<Func<TSource, TKey>> keySelector, OrderBy order)
        {
            if (source is ILazyQuery<TSource>)
            {
                ILazyQuery<TSource> lazy = (ILazyQuery<TSource>)source;
                if (order == MPExtended.Services.MediaAccessService.Interfaces.OrderBy.Asc)
                    return lazy.ThenBy(keySelector);
                return lazy.ThenByDescending(keySelector);
            }

            var comp = keySelector.Compile();
            if (order == MPExtended.Services.MediaAccessService.Interfaces.OrderBy.Asc)
                return Enumerable.ThenBy(source, comp);
            return Enumerable.ThenByDescending(source, comp);
        }

        // Allow easy sorting from MediaAccessService.cs
        public static IOrderedEnumerable<T> SortMediaItemList<T>(this IEnumerable<T> list, SortBy sort, OrderBy order)
        {
            try
            {
                switch (sort)
                {
                    // generic
                    case SortBy.Title:
                        return list.OrderBy(x => ((ITitleSortable)x).Title, order);
                    case SortBy.DateAdded:
                        return list.OrderBy(x => ((IDateAddedSortable)x).DateAdded, order);
                    case SortBy.Year:
                        return list.OrderBy(x => ((IYearSortable)x).Year, order);
                    case SortBy.Genre:
                        return list.OrderBy(x => ((IGenreSortable)x).Genres.First(), order);
                    case SortBy.Rating:
                        return list.OrderBy(x => ((IRatingSortable)x).Rating, order);
                    case SortBy.UserDefinedCategories:
                        return list.OrderBy(x => ((ICategorySortable)x).UserDefinedCategories.First(), order);

                    // music
                    case SortBy.MusicTrackNumber:
                        return list.OrderBy(x => ((IMusicTrackNumberSortable)x).TrackNumber, order);
                    case SortBy.MusicComposer:
                        return list.OrderBy(x => ((IMusicComposerSortable)x).Composer.First(), order);

                    // tv
                    case SortBy.TVEpisodeNumber:
                        return list.OrderBy(x => ((ITVEpisodeNumberSortable)x).SeasonId, order).ThenBy(x => ((ITVEpisodeNumberSortable)x).EpisodeNumber, order);
                    case SortBy.TVSeasonNumber:
                        return list.OrderBy(x => ((ITVSeasonNumberSortable)x).SeasonNumber, order);

                    // picture
                    case SortBy.PictureDateTaken:
                        return list.OrderBy(x => ((IPictureDateTakenSortable)x).DateTaken, order);
                }

                // this can't be reached but the compiler is stupid
                throw new Exception();
            }
            catch (InvalidCastException ex)
            {
                Log.Warn("Tried to do invalid sorting", ex);
                throw new Exception("Sorting on this property is not supported for this media type");
            }
        }
    }

    internal static class WebMediaItemExtensionMethods
    {
        public static ConcreteWebMediaItem ToWebMediaItem(this WebMediaItem item)
        {
            var x = new ConcreteWebMediaItem
            {
                Id = item.Id,
                DateAdded = item.DateAdded,
                Path = item.Path,
                Type = item.Type
            };
            return x;
        }
    }
}