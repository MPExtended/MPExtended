#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MPExtended.Services.MediaAccessService
{
    internal static class IEnumerableExtensionMethods
    {
        // Take a range from the list
        public static IEnumerable<T> TakeRange<T>(this IEnumerable<T> source, int start, int end)
        {
            int count = end - start + 1;
            return source.Skip(start).Take(count);
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
    }

    internal static class IQueryableExtensionMethods
    {
        // Take a range from the list
        public static IQueryable<T> TakeRange<T>(this IQueryable<T> source, int start, int end)
        {
            int count = end - start + 1;
            return source.Skip(start).Take(count);
        }

        // Some special filter methods
        public static IQueryable<T> FilterGenre<T>(this IQueryable<T> list, string genre) where T : IGenreSortable
        {
            if (genre != null)
                return list.Where(x => ((IGenreSortable)x).Genres.Contains(genre));

            return list;
        }

        public static IQueryable<T> FilterActor<T>(this IQueryable<T> list, string actor) where T : IActors
        {
            if (actor != null)
                return list.Where(x => ((IActors)x).Actors.Contains(new WebActor() { Title = actor }));

            return list;
        }

        public static IQueryable<T> FilterStartsWith<T>(this IQueryable<T> list, string startsWith) where T : ITitleSortable
        {
            if (startsWith != null)
                return list.Where(x => x.Title.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase));

            return list;
        }

        public static IQueryable<T> CommonFilter<T>(this IQueryable<T> list, string genre, string actor) where T : IGenreSortable, IActors
        {
            return FilterGenre(FilterActor(list, actor), genre);
        }

        public static IQueryable<T> CommonFilter<T>(this IQueryable<T> list, string genre, string actor, string startsWith) where T : IGenreSortable, IActors, ITitleSortable
        {
            return FilterStartsWith(FilterGenre(FilterActor(list, actor), genre), startsWith);
        }

        // Easy aliases for ordering and sorting
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, WebSortOrder order)
        {
            if (order == WebSortOrder.Asc)
                return Queryable.OrderBy(source, keySelector);
            return Queryable.OrderByDescending(source, keySelector);
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, WebSortOrder order)
        {
            if (order == WebSortOrder.Asc)
                return Queryable.ThenBy(source, keySelector);
            return Queryable.ThenByDescending(source, keySelector);
        }

        // Finalize it
        public static List<T> Finalize<T>(this IQueryable<T> list, int? providerId, ProviderType type) where T : WebObject
        {
            return Finalization.ForList(list, providerId, type);
        }

        public static List<T> Finalize<T>(this IQueryable<T> list, int? providerId, WebMediaType mediatype) where T : WebObject
        {
            return Finalization.ForList(list, providerId, mediatype);
        }

        // Allow easy sorting from MediaAccessService.cs
        public static IOrderedQueryable<T> SortMediaItemList<T>(this IQueryable<T> list, WebSortField? sortInput, WebSortOrder? orderInput)
        {
            return SortMediaItemList<T>(list, sortInput, orderInput, WebSortField.Title, WebSortOrder.Asc);
        }

        public static IOrderedQueryable<T> SortMediaItemList<T>(this IQueryable<T> list, WebSortField? sortInput, WebSortOrder? orderInput, WebSortField defaultSort)
        {
            return SortMediaItemList<T>(list, sortInput, orderInput, defaultSort, WebSortOrder.Asc);
        }

        public static IOrderedQueryable<T> SortMediaItemList<T>(this IQueryable<T> list, WebSortField? sortInput, WebSortOrder? orderInput, WebSortField defaultSort, WebSortOrder defaultOrder)
        {
            // parse arguments
            if (orderInput != null && orderInput != WebSortOrder.Asc && orderInput != WebSortOrder.Desc)
            {
                Log.Warn("Invalid OrderBy value {0} given", orderInput);
                throw new Exception("Invalid OrderBy value specified");
            }
            WebSortField sort = sortInput.HasValue ? sortInput.Value : defaultSort;
            WebSortOrder order = orderInput.HasValue ? orderInput.Value : defaultOrder;

            // do the actual sorting
            try
            {
                switch (sort)
                {
                    // generic
                    case WebSortField.Title:
                        return list.OrderBy(x => ((ITitleSortable)x).Title, order);
                    case WebSortField.DateAdded:
                        return list.OrderBy(x => ((IDateAddedSortable)x).DateAdded, order);
                    case WebSortField.Year:
                        return list.OrderBy(x => ((IYearSortable)x).Year, order);
                    case WebSortField.Genre:
                        return list.OrderBy(x => ((IGenreSortable)x).Genres.First(), order);
                    case WebSortField.Rating:
                        return list.OrderBy(x => ((IRatingSortable)x).Rating, order);
                    case WebSortField.Categories:
                        return list.OrderBy(x => ((ICategorySortable)x).Categories.First().Title, order);
                    case WebSortField.Type:
                        return list.OrderBy(x => ((ITypeSortable)x).Type, order);

                    // music
                    case WebSortField.MusicTrackNumber:
                        return list.OrderBy(x => ((IMusicTrackNumberSortable)x).TrackNumber, order);
                    case WebSortField.MusicComposer:
                        return list.OrderBy(x => ((IMusicComposerSortable)x).Composer.First(), order);

                    // tv
                    case WebSortField.TVEpisodeNumber:
                        return list.OrderBy(x => ((ITVEpisodeNumberSortable)x).SeasonNumber, order).ThenBy(x => ((ITVEpisodeNumberSortable)x).EpisodeNumber, order);
                    case WebSortField.TVSeasonNumber:
                        return list.OrderBy(x => ((ITVSeasonNumberSortable)x).SeasonNumber, order);
                    case WebSortField.TVDateAired:
                        return list.OrderBy(x => ((ITVDateAiredSortable)x).FirstAired, order);

                    // picture
                    case WebSortField.PictureDateTaken:
                        return list.OrderBy(x => ((IPictureDateTakenSortable)x).DateTaken, order);

                    default:
                        Log.Warn("Invalid SortBy value {0}", sortInput);
                        throw new Exception("Sorting on this property is not supported for this media type");
                }
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Tried to do invalid sorting; actual values SortBy={0}, OrderBy={1}", sort, order), ex);
                throw new Exception("Sorting on this property is not supported for this media type");
            }
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> list, string filter)
        {
            String filterRaw = System.Web.HttpUtility.HtmlDecode(filter);
            filterRaw = filterRaw.Replace("\\", "");

            if (filter != null)
            {
                List<WebFilter> filters;
                if (filter.StartsWith("["))
                {
                    filters = JsonConvert.DeserializeObject<List<WebFilter>>(filterRaw);
                }
                else
                {
                    filters = new List<WebFilter>();
                    filters.Add(JsonConvert.DeserializeObject<WebFilter>(filterRaw));
                }
                //TODO: filter the list here
            }
            return list;
        }
    }

    internal static class IOrderedQueryableExtensionMethods
    {
        public static List<T> Finalize<T>(this IOrderedQueryable<T> list, int? providerId, ProviderType type) where T : WebObject
        {
            return Finalization.ForList(list, providerId, type);
        }

        public static List<T> Finalize<T>(this IOrderedQueryable<T> list, int? providerId, WebMediaType mediatype) where T : WebObject
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
            return Finalization.ForItem(item, provider, mediatype.ToProviderType());
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