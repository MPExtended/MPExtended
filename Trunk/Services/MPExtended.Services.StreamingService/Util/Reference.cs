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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;

namespace MPExtended.Services.StreamingService.Util {
    /* Thanks to StackOverflow: http://stackoverflow.com/questions/2980463/how-do-i-assign-by-reference-to-a-class-field-in-c
     * 
     * Example:
     *   int y = 123;
     *   x = new Reference<int>( () => y, z => { y=z; } );
     * </code>
     */
    internal sealed class Reference<T> {
        private readonly Func<T> getMethod;
        private readonly Action<T> setMethod;

        public Reference(Func<T> get, Action<T> set) {
            this.getMethod = get;
            this.setMethod = set;
        }

        public T Value { 
            get { 
                return getMethod();
            } 
            set {
                setMethod(value);
            } 
        }
    }
}
