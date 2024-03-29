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

using System.Collections.Generic;
using System.Linq;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService
{
    internal static class Finalization
    {
        public static List<T> ForList<T>(IEnumerable<T> list, int? provider, ProviderType providerType) where T : WebObject
        {
            // do not error out on null result
            if (list == null)
                return null;

            // special-case LazyQuery here again: execute the query just once instead of a lot of times
            var operList = list.ToList();

            if (operList.Count() == 0)
                return new List<T>();

            int realProvider = ProviderHandler.GetProviderId(providerType, provider);
            bool isArtwork = operList.First() is IArtwork;
            bool isActors = operList.First() is IActors;
            bool isMovieActors = operList.First() is IMovieActors;
            bool isTVShowActors = operList.First() is ITVShowActors;
            bool isGuestStars = operList.First() is IGuestStars;
            bool isCollections = operList.First() is ICollections;
            bool isArtists = operList.First() is IArtists;
            bool isAlbumArtist = operList.First() is IAlbumArtist;
            bool isCategory = operList.First() is ICategorySortable;

            List<T> retlist = new List<T>();
            foreach (T item in operList)
            {
                item.PID = realProvider;

                if (isArtwork)
                {
                    (item as IArtwork).Artwork = (item as IArtwork).Artwork.Select(x => new WebArtwork()
                    {
                        Offset = x.Offset,
                        Type = x.Type,
                        Filetype = x.Filetype,
                        Id = x.Id,
                        Rating = x.Rating
                    })
                        .OrderByDescending(x => x.Rating)
                        .ToList();
                }

                if (isActors)
                {
                    (item as IActors).Actors = (item as IActors).Actors.Select(x => new WebActor()
                    {
                        PID = realProvider,
                        Title = x.Title,
                        Birth = x.Birth,
                        Death = x.Death,
                        Biography = x.Biography,
                        Artwork = x.Artwork.Select(y => new WebArtwork()
                                  {
                                    Offset = y.Offset,
                                    Type = y.Type,
                                    Filetype = y.Filetype,
                                    Id = y.Id,
                                    Rating = y.Rating
                                  })
                                  .OrderByDescending(y => y.Rating)
                                  .ToList(),
                        ExternalId = x.ExternalId.Select(y => new WebExternalId()
                                  {
                                    Id = y.Id,
                                    Site = y.Site
                                  })
                                  .ToList()
                    }).ToList();
                }

                if (isMovieActors)
                {
                    (item as IMovieActors).Actors = (item as IMovieActors).Actors.Select(x => new WebMovieActor()
                    {
                        PID = realProvider,
                        Title = x.Title,
                        Birth = x.Birth,
                        Death = x.Death,
                        Biography = x.Biography,
                        Artwork = x.Artwork.Select(y => new WebArtwork()
                                  {
                                    Offset = y.Offset,
                                    Type = y.Type,
                                    Filetype = y.Filetype,
                                    Id = y.Id,
                                    Rating = y.Rating
                                  })
                                  .OrderByDescending(y => y.Rating)
                                  .ToList(),
                        ExternalId = x.ExternalId.Select(y => new WebExternalId()
                                  {
                                    Id = y.Id,
                                    Site = y.Site
                                  })
                                  .ToList()
                    }).ToList();
                }

                if (isTVShowActors)
                {
                    (item as ITVShowActors).Actors = (item as ITVShowActors).Actors.Select(x => new WebTVShowActor()
                    {
                        PID = realProvider,
                        Title = x.Title,
                        Birth = x.Birth,
                        Death = x.Death,
                        Biography = x.Biography,
                        Artwork = x.Artwork.Select(y => new WebArtwork()
                                  {
                                    Offset = y.Offset,
                                    Type = y.Type,
                                    Filetype = y.Filetype,
                                    Id = y.Id,
                                    Rating = y.Rating
                                  })
                                  .OrderByDescending(y => y.Rating)
                                  .ToList(),
                        ExternalId = x.ExternalId.Select(y => new WebExternalId()
                                  {
                                    Id = y.Id,
                                    Site = y.Site
                                  })
                                  .ToList()
                    }).ToList();
                }

                if (isGuestStars)
                {
                    (item as IGuestStars).GuestStars = (item as IGuestStars).GuestStars.Select(x => new WebTVShowActor()
                    {
                        PID = realProvider,
                        Title = x.Title,
                        Birth = x.Birth,
                        Death = x.Death,
                        Biography = x.Biography,
                        Artwork = x.Artwork.Select(y => new WebArtwork()
                                  {
                                    Offset = y.Offset,
                                    Type = y.Type,
                                    Filetype = y.Filetype,
                                    Id = y.Id,
                                    Rating = y.Rating
                                  })
                                  .OrderByDescending(y => y.Rating)
                                  .ToList(),
                        ExternalId = x.ExternalId.Select(y => new WebExternalId()
                                  {
                                    Id = y.Id,
                                    Site = y.Site
                                  })
                                  .ToList()
                    }).ToList();
                }

                if (isCollections)
                {
                    (item as ICollections).Collections = (item as ICollections).Collections.Select(x => new WebCollection()
                    {
                        PID = realProvider,
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Artwork = x.Artwork.Select(y => new WebArtwork()
                                  {
                                    Offset = y.Offset,
                                    Type = y.Type,
                                    Filetype = y.Filetype,
                                    Id = y.Id,
                                    Rating = y.Rating
                                  })
                                  .OrderByDescending(y => y.Rating)
                                  .ToList()
                    }).ToList();
                }

                if (isArtists)
                {
                    (item as IArtists).Artists = (item as IArtists).Artists.Select(x => new WebMusicArtistBasic()
                    {
                        PID = realProvider,
                        Id = x.Id,
                        Title = x.Title,
                        HasAlbums = x.HasAlbums,
                        FanartCount = x.FanartCount,
                        Artwork = x.Artwork.Select(y => new WebArtwork()
                                  {
                                    Offset = y.Offset,
                                    Type = y.Type,
                                    Filetype = y.Filetype,
                                    Id = y.Id,
                                    Rating = y.Rating
                                  })
                                  .OrderByDescending(y => y.Rating)
                                  .ToList()
                    }).ToList();
                }

                if (isAlbumArtist)
                {
                    (item as IAlbumArtist).AlbumArtistObject = new WebMusicArtistBasic()
                    {
                        PID = item.PID,
                        Id = (item as IAlbumArtist).AlbumArtistObject.Id,
                        Title = (item as IAlbumArtist).AlbumArtistObject.Title,
                        HasAlbums = (item as IAlbumArtist).AlbumArtistObject.HasAlbums,
                        FanartCount = (item as IAlbumArtist).AlbumArtistObject.FanartCount,
                        Artwork = (item as IAlbumArtist).AlbumArtistObject.Artwork.Select(x => new WebArtwork()
                        {
                          Offset = x.Offset,
                          Type = x.Type,
                          Filetype = x.Filetype,
                          Id = x.Id,
                          Rating = x.Rating
                        })
                          .OrderByDescending(x => x.Rating)
                          .ToList()
                    };
                }

                if (isCategory)
                {
                    (item as ICategorySortable).Categories = (item as ICategorySortable).Categories.Select(x => new WebCategory()
                    {
                        PID = realProvider,
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description
                    }).ToList();
                }

                retlist.Add(item);
            }

            return retlist;
        }

        public static List<T> ForList<T>(IEnumerable<T> list, int? provider, WebMediaType providerType) where T : WebObject
        {
            return ForList(list, provider, providerType.ToProviderType());
        }

        public static T ForItem<T>(T item, int? provider, ProviderType providerType) where T : WebObject
        {
            if (item == null)
                return null;

            item.PID = ProviderHandler.GetProviderId(providerType, provider);
            if (item is IArtwork)
            {
                (item as IArtwork).Artwork = (item as IArtwork).Artwork.Select(x => new WebArtwork()
                {
                    Offset = x.Offset,
                    Type = x.Type,
                    Filetype = x.Filetype,
                    Id = x.Id,
                    Rating = x.Rating
                }).OrderByDescending(x => x.Rating)
                  .ToList();
            }

            if (item is IActors)
            {
                (item as IActors).Actors = (item as IActors).Actors.Select(x => new WebActor()
                {
                    PID = item.PID,
                    Title = x.Title,
                    Birth = x.Birth,
                    Death = x.Death,
                    Biography = x.Biography,
                    Artwork = x.Artwork.Select(y => new WebArtwork()
                    {
                        Offset = y.Offset,
                        Type = y.Type,
                        Filetype = y.Filetype,
                        Id = y.Id,
                        Rating = y.Rating
                    })
                    .OrderByDescending(y => y.Rating)
                    .ToList(),
                    ExternalId = x.ExternalId.Select(y => new WebExternalId()
                    {
                        Id = y.Id,
                        Site = y.Site
                    })
                    .ToList()
                }).ToList();
            }

            if (item is IMovieActors)
            {
                (item as IMovieActors).Actors = (item as IMovieActors).Actors.Select(x => new WebMovieActor()
                {
                    PID = item.PID,
                    Title = x.Title,
                    Birth = x.Birth,
                    Death = x.Death,
                    Biography = x.Biography,
                    Artwork = x.Artwork.Select(y => new WebArtwork()
                    {
                        Offset = y.Offset,
                        Type = y.Type,
                        Filetype = y.Filetype,
                        Id = y.Id,
                        Rating = y.Rating
                    })
                    .OrderByDescending(y => y.Rating)
                    .ToList(),
                    ExternalId = x.ExternalId.Select(y => new WebExternalId()
                    {
                        Id = y.Id,
                        Site = y.Site
                    })
                    .ToList()
                }).ToList();
            }

            if (item is ITVShowActors)
            {
                (item as ITVShowActors).Actors = (item as ITVShowActors).Actors.Select(x => new WebTVShowActor()
                {
                    PID = item.PID,
                    Title = x.Title,
                    Birth = x.Birth,
                    Death = x.Death,
                    Biography = x.Biography,
                    Artwork = x.Artwork.Select(y => new WebArtwork()
                    {
                        Offset = y.Offset,
                        Type = y.Type,
                        Filetype = y.Filetype,
                        Id = y.Id,
                        Rating = y.Rating
                    })
                    .OrderByDescending(y => y.Rating)
                    .ToList(),
                    ExternalId = x.ExternalId.Select(y => new WebExternalId()
                    {
                        Id = y.Id,
                        Site = y.Site
                    })
                    .ToList()
                }).ToList();
            }

            if (item is IGuestStars)
            {
                (item as IGuestStars).GuestStars = (item as IGuestStars).GuestStars.Select(x => new WebTVShowActor()
                {
                    PID = item.PID,
                    Title = x.Title,
                    Birth = x.Birth,
                    Death = x.Death,
                    Biography = x.Biography,
                    Artwork = x.Artwork.Select(y => new WebArtwork()
                    {
                        Offset = y.Offset,
                        Type = y.Type,
                        Filetype = y.Filetype,
                        Id = y.Id,
                        Rating = y.Rating
                    })
                    .OrderByDescending(y => y.Rating)
                    .ToList(),
                    ExternalId = x.ExternalId.Select(y => new WebExternalId()
                    {
                        Id = y.Id,
                        Site = y.Site
                    })
                    .ToList()
                }).ToList();
            }

            if (item is ICategorySortable)
            {
                (item as ICategorySortable).Categories = (item as ICategorySortable).Categories.Select(x => new WebCategory()
                {
                    PID = item.PID,
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description
                }).ToList();
            }

            if (item is ICollections)
            {
                (item as ICollections).Collections = (item as ICollections).Collections.Select(x => new WebCollection()
                {
                    PID = item.PID,
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Artwork = x.Artwork.Select(y => new WebArtwork()
                    {
                      Offset = y.Offset,
                      Type = y.Type,
                      Filetype = y.Filetype,
                      Id = y.Id,
                      Rating = y.Rating
                    })
                    .OrderByDescending(y => y.Rating)
                    .ToList()
                }).ToList();
            }

            if (item is IArtists)
            {
                (item as IArtists).Artists = (item as IArtists).Artists.Select(x => new WebMusicArtistBasic()
                {
                    PID = item.PID,
                    Id = x.Id,
                    Title = x.Title,
                    HasAlbums = x.HasAlbums,
                    FanartCount = x.FanartCount,
                    Artwork = x.Artwork.Select(y => new WebArtwork()
                    {
                      Offset = y.Offset,
                      Type = y.Type,
                      Filetype = y.Filetype,
                      Id = y.Id,
                      Rating = y.Rating
                    })
                    .OrderByDescending(y => y.Rating)
                    .ToList()
                }).ToList();
            }

            if (item is IAlbumArtist)
            {
                (item as IAlbumArtist).AlbumArtistObject = new WebMusicArtistBasic()
                {
                    PID = item.PID,
                    Id = (item as IAlbumArtist).AlbumArtistObject.Id,
                    Title = (item as IAlbumArtist).AlbumArtistObject.Title,
                    HasAlbums = (item as IAlbumArtist).AlbumArtistObject.HasAlbums,
                    FanartCount = (item as IAlbumArtist).AlbumArtistObject.FanartCount,
                    Artwork = (item as IAlbumArtist).AlbumArtistObject.Artwork.Select(x => new WebArtwork()
                    {
                      Offset = x.Offset,
                      Type = x.Type,
                      Filetype = x.Filetype,
                      Id = x.Id,
                      Rating = x.Rating
                    })
                    .OrderByDescending(x => x.Rating)
                    .ToList()
                };
            }

            return item;
        }
    }
}
