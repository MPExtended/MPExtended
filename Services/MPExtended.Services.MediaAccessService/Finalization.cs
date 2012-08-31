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
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.Common.Interfaces;

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
                    }).ToList();
                }

                if (isActors)
                {
                    (item as IActors).Actors = (item as IActors).Actors.Select(x => new WebActor()
                    {
                        PID = realProvider,
                        Title = x.Title
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
                }).ToList();
            }

            if (item is ICategorySortable)
            {
                (item as ICategorySortable).Categories = (item as ICategorySortable).Categories.Select(x => new WebCategory()
                {
                    Id = x.Id,
                    PID = item.PID,
                    Title = x.Title
                }).ToList();
            }

            return item;
        }
    }
}
