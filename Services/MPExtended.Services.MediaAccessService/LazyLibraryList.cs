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
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    internal class LazyLibraryList<T> : IEnumerable<T> where T : ILibrary
    {
        private IDictionary<int, Lazy<T, IDictionary<string, object>>> items = new Dictionary<int, Lazy<T, IDictionary<string, object>>>();

        public LazyLibraryList(IDictionary<int, Lazy<T, IDictionary<string, object>>> dict) 
        {
            items = dict;
        }

        public void Add(int key, Lazy<T, IDictionary<string, object>> value) 
        {
            items[key] = value;
        }

        public T this[int key]
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

        public T GetValue(int key)
        {
            if (!items.ContainsKey(key))
            {
                Log.Error("Tried to get library for unknown id {0}", key);
                return default(T);
            }

            lock (items)
            {
                if (!items[key].IsValueCreated)
                {
                    ILibrary item = (ILibrary)(items[key].Value);
                    item.Init();
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
    }
}
