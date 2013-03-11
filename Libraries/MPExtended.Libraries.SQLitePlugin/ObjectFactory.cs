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
using System.Data.SQLite;
using System.Text;

namespace MPExtended.Libraries.SQLitePlugin
{
    // TODO: remove new() constraint
    internal class ObjectFactory<T> where T : new()
    {
        private Delegates<T>.FillMethod fill;
        private Delegates<T>.CreateMethod create;

        private void SetFill(Delegates<T>.FillMethod fill)
        {
            this.fill = fill;
        }

        private void SetCreate(Delegates<T>.CreateMethod create)
        {
            this.create = create;
        }

        public T CreateObject(SQLiteDataReader reader)
        {
            if (this.fill != null)
            {
                T obj = new T();
                fill(obj, reader);
                return obj;
            }

            if (this.create != null)
            {
                return create(reader);
            }

            throw new InvalidOperationException();
        }

        public static ObjectFactory<T> FromFill(Delegates<T>.FillMethod fill)
        {
            ObjectFactory<T> obj = new ObjectFactory<T>();
            obj.SetFill(fill);
            return obj;
        }

        public static ObjectFactory<T> FromCreate(Delegates<T>.CreateMethod create)
        {
            ObjectFactory<T> obj = new ObjectFactory<T>();
            obj.SetCreate(create);
            return obj;
        }
    }
}
