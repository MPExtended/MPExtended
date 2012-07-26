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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MPExtended.Libraries.Service.Config
{
    [DataContract(Name = "ConfigType", Namespace = "http://mpextended.github.com/schema/config/MediaAccess/1")]
    public enum ConfigType 
    { 
        [EnumMember]
        File,
        [EnumMember]
        Folder,
        [EnumMember]
        Text,
        [EnumMember]
        Number,
        [EnumMember]
        Boolean 
    }

    [DataContract(Name = "PluginConfigItem", Namespace = "http://mpextended.github.com/schema/config/MediaAccess/1")]
    public class PluginConfigItem
    {
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
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

    [DataContract(Name = "DefaultPlugins", Namespace = "http://mpextended.github.com/schema/config/MediaAccess/1")]
    public class DefaultPlugins
    {
        [DataMember]
        public string TVShow { get; set; }
        [DataMember]
        public string Movie { get; set; }
        [DataMember]
        public string Music { get; set; }
        [DataMember]
        public string Picture { get; set; }
        [DataMember]
        public string Filesystem { get; set; }

        public DefaultPlugins()
        {
        }
    }

    [DataContract(Name = "MediaAccess", Namespace = "http://mpextended.github.com/schema/config/MediaAccess/1")]
    public class MediaAccess
    {
        [DataMember]
        public DefaultPlugins DefaultPlugins { get; set; }
        [DataMember]
        public PluginConfigDictionary PluginConfiguration { get; set; }

        public MediaAccess()
        {
            DefaultPlugins = new DefaultPlugins();
            PluginConfiguration = new PluginConfigDictionary();
        }
    }
}
