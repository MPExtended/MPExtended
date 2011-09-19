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
using System.Data.SQLite;
using System.Text;

namespace MPExtended.Libraries.SQLitePlugin
{
    internal class ObjectFactory<T> where T : new()
    {
        private LazyQuery<T>.FillMethod fill;
        private LazyQuery<T>.CreateMethod create;
        private LazyQuery<T>.ListCreateMethod list;

        private void SetFill(LazyQuery<T>.FillMethod fill)
        {
            this.fill = fill;
        }

        private void SetCreate(LazyQuery<T>.CreateMethod create)
        {
            this.create = create;
        }

        private void SetList(LazyQuery<T>.ListCreateMethod list)
        {
            this.list = list;
        }

        public List<T> CreateObjects(SQLiteDataReader reader)
        {
            if (this.fill != null)
            {
                T obj = new T();
                fill(obj, reader);
                return new List<T>() { obj };
            }

            if (this.create != null)
            {
                return new List<T>() { create(reader) };
            }

            if (this.list != null)
            {
                return list(reader);
            }

            throw new InvalidOperationException();
        }

        public static ObjectFactory<T> FromFill(LazyQuery<T>.FillMethod fill)
        {
            ObjectFactory<T> obj = new ObjectFactory<T>();
            obj.SetFill(fill);
            return obj;
        }

        public static ObjectFactory<T> FromCreate(LazyQuery<T>.CreateMethod create)
        {
            ObjectFactory<T> obj = new ObjectFactory<T>();
            obj.SetCreate(create);
            return obj;
        }

        public static ObjectFactory<T> FromList(LazyQuery<T>.ListCreateMethod list)
        {
            ObjectFactory<T> obj = new ObjectFactory<T>();
            obj.SetList(list);
            return obj;
        }
    }
}
