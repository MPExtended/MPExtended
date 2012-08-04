#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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

namespace MPExtended.Libraries.Service.Util
{
    public class AnonymousComparer<T> : Comparer<T>
    {
        private Func<T, T, int> compareFunction;

        public AnonymousComparer(Func<T, T, int> compare)
        {
            this.compareFunction = compare;
        }

        public override int Compare(T x, T y)
        {
            return compareFunction(x, y);
        }
    }

    public class AnonymousComparer 
    {
        public static IComparer<T> FromLambda<T>(Func<T, T, int> compare)
        {
            return new AnonymousComparer<T>(compare);
        }
    }

    public class AnonymousEqualityComparer<T> : EqualityComparer<T>
    {
        private Func<T, T, bool> equalsFunction;
        private Func<T, int> getHashCodeFunction;

        public AnonymousEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            this.equalsFunction = equals;
            this.getHashCodeFunction = getHashCode;
        }

        public override bool Equals(T x, T y)
        {
            return equalsFunction(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return getHashCodeFunction(obj);
        }
    }

    public class AnonymousEqualityComparer
    {
        public static IEqualityComparer<T> FromLambda<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            return new AnonymousEqualityComparer<T>(equals, getHashCode);
        }
    }
}
