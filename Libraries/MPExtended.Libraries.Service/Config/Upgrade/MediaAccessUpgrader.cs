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
using System.Text;
using System.Xml.Linq;

namespace MPExtended.Libraries.Service.Config.Upgrade
{
    internal class MediaAccessUpgrader : AttemptConfigUpgrader<MediaAccess>
    {
        protected override MediaAccess DoUpgrade()
        {
            var file = XElement.Load(OldPath);
            var model = new MediaAccess();

            model.DefaultPlugins = new DefaultPlugins()
            {
                Filesystem = file.Element("defaultPlugins").Element("filesystem").Value,
                Movie = file.Element("defaultPlugins").Element("movie").Value,
                Music = file.Element("defaultPlugins").Element("music").Value,
                Picture = file.Element("defaultPlugins").Element("picture").Value,
                TVShow = file.Element("defaultPlugins").Element("tvshow").Value,
            };

            model.PluginConfiguration = new PluginConfigDictionary();
            foreach (XElement plugin in file.Element("pluginConfiguration").Elements("plugin"))
            {
                model.PluginConfiguration[plugin.Attribute("name").Value] = new List<PluginConfigItem>();
                foreach (var x in plugin.Elements())
                {
                    ConfigType type = (ConfigType)Enum.Parse(typeof(ConfigType), x.Attribute("type").Value, true);
                    string value = type == ConfigType.File || type == ConfigType.Folder ? TransformationCallbacks.FolderSubstitution(x.Value) : x.Value;
                    model.PluginConfiguration[plugin.Attribute("name").Value].Add(new PluginConfigItem()
                    {
                        DisplayName = x.Attribute("displayname").Value,
                        Name = x.Name.LocalName,
                        Type = type,
                        Value = value
                    });
                }
            }

            return model;
        }
    }
}
