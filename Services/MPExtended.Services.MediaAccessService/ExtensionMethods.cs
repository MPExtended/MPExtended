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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
//using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService
{
    internal static class IEnumerableExtensionMethods
    {
        // Take a range from the list for returning
        public static IEnumerable<T> TakeRange<T>(this IEnumerable<T> source, int start, int end)
        {
            int count = end - start + 1;

            if (source is List<T>)
                return ((List<T>)source).GetRange(start, count);
            if (source is ILazyQuery<T>)
                return ((ILazyQuery<T>)source).GetRange(start, count);
            return source.Skip(start).Take(count);
        }

        // Some special filter methods
        public static IEnumerable<T> FilterGenre<T>(this IEnumerable<T> list, string genre) where T : IGenreSortable
        {
            if (genre != null)
                return Where(list, x => ((IGenreSortable)x).Genres.Contains(genre));

            return list;
        }

        public static IEnumerable<T> FilterCategory<T>(this IEnumerable<T> list, string category) where T : ICategorySortable
        {
            if (category != null)
                return Where(list, x => ((ICategorySortable)x).UserDefinedCategories.Contains(category));

            return list;
        }

        public static IEnumerable<T> FilterActor<T>(this IEnumerable<T> list, string actor) where T : IActors
        {
            if (actor != null)
                return Where(list, x => ((IActors)x).Actors.Contains(new WebActor() { Name = actor }));

            return list;
        }

        public static IEnumerable<T> CommonFilter<T>(this IEnumerable<T> list, string genre, string category) where T : IGenreSortable, ICategorySortable
        {
            return FilterCategory(FilterGenre(list, genre), category);
        }

        public static IEnumerable<T> CommonFilter<T>(this IEnumerable<T> list, string genre, string category, string actor) where T : IGenreSortable, ICategorySortable, IActors
        {
            return FilterCategory(FilterGenre(FilterActor(list, actor), genre), category);
        }

        // Take advantage of lazy queries
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Expression<Func<T, bool>> predicate)
        {
            if (source is ILazyQuery<T>)
                return ((ILazyQuery<T>)source).Where(predicate);
            return Enumerable.Where(source, predicate.Compile());
        }

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
    
        // Finalize it
        public static List<T> Finalize<T>(this IEnumerable<T> list, int? providerId, ProviderType type) where T : WebObject
        {
            return Finalization.ForList(list, providerId, type);
        }

        public static List<T> Finalize<T>(this IEnumerable<T> list, int? providerId, WebMediaType mediatype) where T : WebObject
        {
            return Finalization.ForList(list, providerId, mediatype);
        }

        // Allow easy sorting from MediaAccessService.cs
        public static IOrderedEnumerable<T> SortMediaItemList<T>(this IEnumerable<T> list, SortBy? sortInput, OrderBy? orderInput)
        {
            // parse arguments
            if (orderInput != null && orderInput != Interfaces.OrderBy.Asc && orderInput != Interfaces.OrderBy.Desc)
            {
                Log.Warn("Invalid OrderBy value {0} given", orderInput);
                throw new Exception("Invalid OrderBy value specified");
            }
            SortBy sort = sortInput.HasValue ? sortInput.Value : SortBy.Title;
            OrderBy order = orderInput.HasValue ? orderInput.Value : Interfaces.OrderBy.Asc;

            // do the actual sorting
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
                    case SortBy.Type:
                        return list.OrderBy(x => ((ITypeSortable)x).Type, order);
                    case SortBy.Name:
                        return list.OrderBy(x => ((INameSortable)x).Name, order);

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
                    case SortBy.TVDateAired:
                        return list.OrderBy(x => ((ITVDateAiredSortable)x).FirstAired, order);

                    // picture
                    case SortBy.PictureDateTaken:
                        return list.OrderBy(x => ((IPictureDateTakenSortable)x).DateTaken, order);

                    default:
                        Log.Warn("Invalid SortBy value {0}", sortInput);
                        throw new Exception("Sorting on this property is not supported for this media type");
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Tried to do invalid sorting", ex);
                throw new Exception("Sorting on this property is not supported for this media type");
            }
        }
    }

    internal static class IOrderedEnumerableExtensionMethods
    {
        public static List<T> Finalize<T>(this IOrderedEnumerable<T> list, int? providerId, ProviderType type) where T : WebObject
        {
            return Finalization.ForList(list, providerId, type);
        }

        public static List<T> Finalize<T>(this IOrderedEnumerable<T> list, int? providerId, WebMediaType mediatype) where T : WebObject
        {
            return Finalization.ForList(list, providerId, mediatype);
        }
    }

    internal static class WebObjectExtensionMethods
    {
        public static T Finalize<T>(this T item, int? provider, ProviderType type) where T : WebObject
        {
            return Finalization.ForItem(item, provider, type);
        }

        public static T Finalize<T>(this T item, int? provider, WebMediaType mediatype) where T : WebObject
        {
            return Finalization.ForItem(item, provider, mediatype);
        }
    }

    internal static class WebMediaItemExtensionMethods
    {
        public static WebMediaItem ToWebMediaItem(this WebMediaItem item)
        {
            var x = new WebMediaItem
            {
                Id = item.Id,
                DateAdded = item.DateAdded,
                Path = item.Path,
                PID = item.PID,
                Type = item.Type
            };
            return x;
        }
    }

    internal static class WebFilesystemItemExtensionMethods
    {
        public static WebFilesystemItem ToWebFilesystemItem(this WebFilesystemItem item)
        {
            return new WebFilesystemItem()
            {
                DateAdded = item.DateAdded,
                Id = item.Id,
                LastAccessTime = item.LastAccessTime,
                LastModifiedTime = item.LastModifiedTime,
                Path = item.Path,
                PID = item.PID,
                Title = item.Title,
                Type = item.Type
            };
        }
    }

    internal static class WebMediaTypeExtensionMethods
    {
        public static ProviderType ToProviderType(this WebMediaType mediatype)
        {
            switch (mediatype)
            {
                case WebMediaType.File:
                    return ProviderType.Filesystem;
                case WebMediaType.Movie:
                    return ProviderType.Movie;
                case WebMediaType.MusicAlbum:
                case WebMediaType.MusicArtist:
                case WebMediaType.MusicTrack:
                    return ProviderType.Music;
                case WebMediaType.Picture:
                    return ProviderType.Picture;
                case WebMediaType.TVEpisode:
                case WebMediaType.TVSeason:
                case WebMediaType.TVShow:
                    return ProviderType.TVShow;
                default:
                    throw new ArgumentException();
            }
        }
    }
}