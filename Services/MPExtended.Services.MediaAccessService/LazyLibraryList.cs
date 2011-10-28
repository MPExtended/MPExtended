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
using System.Reflection;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal class LazyLibraryList<T> : ILibraryList<T> where T : ILibrary
    {
        private IDictionary<int, Lazy<T, IDictionary<string, object>>> items = new Dictionary<int, Lazy<T, IDictionary<string, object>>>();
        private ProviderType type;
        private IDictionary<int, bool> didInitialize = new Dictionary<int, bool>();

        public LazyLibraryList(IDictionary<int, Lazy<T, IDictionary<string, object>>> dict, ProviderType type) 
        {
            items = dict;
            this.type = type;
        }

        public void Add(int key, Lazy<T, IDictionary<string, object>> value) 
        {
            items[key] = value;
        }

        public T this[int? key]
        {
            get
            {
                return GetValue(key);
            }
        }

        public ICollection<int> Keys
        {
            get
            {
                return items.Keys;
            }
        }

        public T GetValue(int? passedId)
        {
            int key = passedId.HasValue ? passedId.Value : ProviderHandler.GetDefaultProvider(type);

            if (!items.ContainsKey(key))
            {
                Log.Error("Tried to get library for unknown id {0}", key);
                return default(T);
            }

            lock (didInitialize)
            {
                if (!didInitialize.ContainsKey(key) || !didInitialize[key]) 
                {
                    ILibrary item = (ILibrary)(items[key].Value);
                    item.Init();
                    didInitialize[key] = true;
                }
            }

            return items[key].Value;
        }

        public Tuple<T, IDictionary<string, object>> GetValueAndMetadata(int key)
        {
            return new Tuple<T, IDictionary<string, object>>(GetValue(key), items[key].Metadata);
        }

        public int Count()
        {
            return items.Count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.Select(x => GetValue(x.Key)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // more specific methods below
        public int GetKeyByName(string name)
        {
            var list = items.Where(x => (string)x.Value.Metadata["Name"] == name);
            if (list.Count() > 0)
            {
                return list.First().Key;
            }

            return 0;
        }

        public List<WebBackendProvider> GetAllAsBackendProvider()
        {
            List<WebBackendProvider> ret = new List<WebBackendProvider>();
            foreach (int key in items.Keys)
            {
                ret.Add(new WebBackendProvider()
                {
                    Name = (string)items[key].Metadata["Name"],
                    Id = (int)items[key].Metadata["Id"],
                    Version = VersionUtil.GetBuildVersion(((Type)(items[key].Metadata["Type"])).Assembly).ToString()
                });
            }
            return ret;
        }

        public IEnumerable<WebSearchResult> SearchAll(string text)
        {
            return items.Keys.SelectMany(key => GetValue(key).Search(text).FillProvider((int)items[key].Metadata["Id"], type));
        }
    }
}
