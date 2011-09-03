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
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;
using MPExtended.Services.MediaAccessService.Interfaces.Shared;

namespace MPExtended.Services.MediaAccessService
{
    internal static class IEnumerableExtensionMethods
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, OrderBy order)
        {
            if (order == MPExtended.Services.MediaAccessService.Interfaces.OrderBy.Asc)
                return source.OrderBy(keySelector);
            return source.OrderByDescending(keySelector);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, OrderBy order)
        {
            if (order == MPExtended.Services.MediaAccessService.Interfaces.OrderBy.Asc)
                return source.ThenBy(keySelector);
            return source.ThenByDescending(keySelector);
        }

        public static IEnumerable<T> GetRange<T>(this IEnumerable<T> source, int index, int count)
        {
            return source.Skip(index).Take(count);
        }

        public static IOrderedEnumerable<T> SortMediaItemList<T>(this IEnumerable<T> list, SortBy sort, OrderBy order)
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
    }
}