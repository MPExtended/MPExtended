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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public Type ResourceType { get; set; }
        public string ResourceName { get; set; }

        public LocalizedDisplayNameAttribute()
        {
        }

        public LocalizedDisplayNameAttribute(Type resourceType, string resourceName)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
        }

        public override string DisplayName
        {
            get
            {
                var pi = ResourceType.GetProperty(ResourceName, BindingFlags.Static | BindingFlags.Public);
                return (string)pi.GetValue(ResourceType, null);
            }
        }
    }
}