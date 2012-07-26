#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MPExtended.Libraries.Service.Config
{
    public enum ConfigType 
    { 
        File, 
        Folder, 
        Text, 
        Number, 
        Boolean 
    }

    public class PluginConfigItem
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ConfigType Type { get; set; }

        public PluginConfigItem()
        {
        }

        public PluginConfigItem(PluginConfigItem old)
        {
            this.Value = old.Value;
            this.Name = old.Name;
            this.DisplayName = old.DisplayName;
            this.Type = old.Type;
        }
    }

    public class DefaultPlugins
    {
        public DefaultPlugins()
        {
        }

        public string TVShow { get; set; }
        public string Movie { get; set; }
        public string Music { get; set; }
        public string Picture { get; set; }
        public string Filesystem { get; set; }
    }

    public class MediaAccess
    {
        public MediaAccess()
        {
        }

        public DefaultPlugins DefaultPlugins { get; set; }
        public Dictionary<string, List<PluginConfigItem>> PluginConfiguration { get; set; }
        public List<string> DisabledPlugins { get; set; }
    }
}
