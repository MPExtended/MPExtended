﻿#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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

using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal static class IEnumerableExtensionMethods
    {
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
                    case WebSortField.NaturalTitle:
                        return list.OrderByNatural(x => ((ITitleSortable)x).Title, order);
                    case WebSortField.DateAdded:
                        return list.OrderBy(x => ((IDateAddedSortable)x).DateAdded, order);
                    case WebSortField.Year:
                        return list.OrderBy(x => ((IYearSortable)x).Year, order);
                    case WebSortField.Genre:
                        if (list.First() is IGenreSortable)
                            return list.OrderBy(x => ((IGenreSortable)x).Genres.First(), order);
                        else if (list.First() is IMovieGenreSortable)
                            return list.OrderBy(x => ((IMovieGenreSortable)x).Genres.First(), order);
                        else // if(list.First() is ITVGenreSortable)
                            return list.OrderBy(x => ((ITVGenreSortable)x).Genres.First(), order);
                    case WebSortField.Rating:
                        return list.OrderBy(x => ((IRatingSortable)x).Rating, order);
                    case WebSortField.Categories:
                        return list.OrderBy(x => ((ICategorySortable)x).Categories.First().Title, order);
                    case WebSortField.Type:
                        return list.OrderBy(x => ((ITypeSortable)x).Type, order);

                    // music
                    case WebSortField.MusicTrackNumber:
                        return list.OrderBy(x => ((IMusicTrackNumberSortable)x).DiscNumber, order).ThenBy(x => ((IMusicTrackNumberSortable)x).TrackNumber, order);
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
                Log.Warn(String.Format("Executing OrderBy(sortBy={0}, orderBy={1}) on list failed, sort parameters are invalid or list iterator failed " + 
                                       "(might be caused by an invalid or unexpected database or a broken query).", sort, order), ex);
                throw new Exception("Failed to load data from source, please see the log file for more details.");
            }
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
                Type = item.Type,
                Pincode = item.Pincode
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
                case WebMediaType.Drive:
                case WebMediaType.Folder:
                case WebMediaType.File:
                    return ProviderType.Filesystem;
                case WebMediaType.Movie:
                case WebMediaType.Collection:
                case WebMediaType.MovieActor:
                case WebMediaType.MovieGenre:
                    return ProviderType.Movie;
                case WebMediaType.MusicAlbum:
                case WebMediaType.MusicArtist:
                case WebMediaType.MusicTrack:
                    return ProviderType.Music;
                case WebMediaType.Picture:
                case WebMediaType.PictureFolder:
                case WebMediaType.MobileVideo:
                    return ProviderType.Picture;
                case WebMediaType.TVEpisode:
                case WebMediaType.TVSeason:
                case WebMediaType.TVShow:
                case WebMediaType.TVShowActor:
                case WebMediaType.TVShowGenre:
                    return ProviderType.TVShow;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
