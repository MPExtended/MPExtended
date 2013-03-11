#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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

namespace MPExtended.Libraries.Service.Composition
{
    public class Plugin<TPlugin>
    {
        public TPlugin Value { get; protected set; }
        public IDictionary<string, object> Metadata { get; protected set; }

        public Plugin(Lazy<TPlugin, IDictionary<string, object>> lazy)
        {
            Value = lazy.Value;
            Metadata = lazy.Metadata;
        }

        public Plugin(TPlugin value, IDictionary<string, object> metadata)
        {
            Value = value;
            Metadata = metadata;
        }
    }
}
