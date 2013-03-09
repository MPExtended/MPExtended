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
using System.Xml.Serialization;

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

    [XmlType(Namespace = "http://mpextended.github.com/schema/config/MediaAccess/1")]
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

    [XmlType(Namespace = "http://mpextended.github.com/schema/config/MediaAccess/1")]
    public class DefaultPlugins
    {
        public string TVShow { get; set; }
        public string Movie { get; set; }
        public string Music { get; set; }
        public string Picture { get; set; }
        public string Filesystem { get; set; }

        public DefaultPlugins()
        {
        }
    }

    [XmlRoot(Namespace = "http://mpextended.github.com/schema/config/MediaAccess/1")]
    public class MediaAccess
    {
        public DefaultPlugins DefaultPlugins { get; set; }
        public PluginConfigDictionary PluginConfiguration { get; set; }

        public MediaAccess()
        {
            DefaultPlugins = new DefaultPlugins();
            PluginConfiguration = new PluginConfigDictionary();
        }
    }
}
