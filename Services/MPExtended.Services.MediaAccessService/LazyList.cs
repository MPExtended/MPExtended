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

namespace MPExtended.Services.MediaAccessService
{
    internal class LazyList<TKey, TValue, TMetadata> : IEnumerable<TValue>
    {
        private IDictionary<TKey, Lazy<TValue, TMetadata>> items = new Dictionary<TKey, Lazy<TValue, TMetadata>>();

        public LazyList(IDictionary<TKey, Lazy<TValue, TMetadata>> dict) 
        {
            items = dict;
        }

        public void Add(TKey key, Lazy<TValue, TMetadata> value) 
        {
            items[key] = value;
        }

        public TValue this[TKey key]
        {
            get
            {
                return items[key].Value;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public int Count()
        {
            return items.Count;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return items.Select(x => x.Value.Value).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
