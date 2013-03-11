#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Reflection;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Shared;
using MPExtended.Libraries.Service.Composition;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    public interface ILibraryList<T> where T : ILibrary
    {
        List<WebBackendProvider> GetAllAsBackendProvider();
        int GetKeyByName(string name);
        IEnumerable<WebSearchResult> SearchAll(string text);
        T this[int? key] { get; }
    }

    internal class LibraryList<T> : ILibraryList<T> where T : ILibrary
    {
        private IDictionary<int, Plugin<T>> items;
        private ProviderType type;

        public LibraryList(IDictionary<int, Plugin<T>> dict, ProviderType providerType) 
        {
            type = providerType;

            items = new Dictionary<int, Plugin<T>>();
            foreach (var plugin in dict)
            {
                try
                {
                    if (plugin.Value.Value.Supported)
                        items[plugin.Key] = plugin.Value;
                }
                catch (Exception ex)
                {
                    string name = plugin.Value.Metadata.ContainsKey("Name") ? (string)plugin.Value.Metadata["Name"] : "<unknown>";
                    Log.Error(String.Format("Failed to load plugin {0}", name), ex);
                }
            }
        }

        public T this[int? key]
        {
            get
            {
                int realKey = key.HasValue ? key.Value : ProviderHandler.GetDefaultProvider(type);

                if (!items.ContainsKey(realKey) || !items[realKey].Value.Supported)
                {
                    Log.Error("Tried to get library for unknown id {0}", key);
                    throw new MethodCallFailedException(String.Format("Tried to get library for unknown id {0}", key));
                }

                return items[realKey].Value;
            }
        }

        // more specific methods below
        public int GetKeyByName(string name)
        {
            var list = items.Where(x => (string)x.Value.Metadata["Name"] == name);
            if (list.Count() > 0)
                return list.First().Key;

            return 0;
        }

        public List<WebBackendProvider> GetAllAsBackendProvider()
        {
            return items.Values
                .Select(x => new WebBackendProvider()
                {
                    Name = (string)x.Metadata["Name"],
                    Id = (int)x.Metadata["Id"],
                    Version = VersionUtil.GetBuildVersion(x.Value.GetType().Assembly).ToString()
                })
                .ToList();
        }

        public IEnumerable<WebSearchResult> SearchAll(string text)
        {
            List<WebSearchResult> originalResults = new List<WebSearchResult>();
            foreach (var library in items)
            {
                try
                {
                    originalResults.AddRange(library.Value.Value.Search(text).Finalize((int)library.Value.Metadata["Id"], type));
                }
                catch (Exception ex)
                {
                    Log.Warn("Search failed in library " + library.Value.Metadata["Name"], ex);
                }
            }

            return originalResults
                .GroupBy(x => x.Id, (key, results) => results.OrderByDescending(res => res.Score).First());
        }
    }
}
