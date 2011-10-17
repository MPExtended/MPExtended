#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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

namespace MPExtended.Libraries.General
{
    public enum ConfigType { File, Folder, Text, Number, Boolean }

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

    public class MediaAccessConfiguration
    {
        public string TVShowPlugin { get; set; }
        public string MoviePlugin { get; set; }
        public string MusicPlugin { get; set; }
        public string PicturePlugin { get; set; }
        public string FilesystemPlugin { get; set; }

        public Dictionary<string, List<PluginConfigItem>> PluginConfiguration { get; set; }

        public MediaAccessConfiguration()
        {
            try
            {
                XElement file = XElement.Load(Configuration.GetPath("MediaAccess.xml"));

                TVShowPlugin = file.Element("plugins").Element("tvShow").Value;
                MoviePlugin = file.Element("plugins").Element("movie").Value;
                MusicPlugin = file.Element("plugins").Element("music").Value;
                PicturePlugin = file.Element("plugins").Element("picture").Value;
                FilesystemPlugin = file.Element("plugins").Element("filesystem").Value;

                PluginConfiguration = new Dictionary<string, List<PluginConfigItem>>();

                foreach (XElement plugin in file.Element("pluginConfiguration").Elements("plugin"))
                {
                        PluginConfiguration[plugin.Attribute("name").Value] = plugin.Elements().Select(x => new PluginConfigItem()
                            {
                                DisplayName = x.Attribute("displayname").Value,
                                Name = x.Name.LocalName,
                                Type = (ConfigType)Enum.Parse(typeof(ConfigType), x.Attribute("type").Value, true),
                                Value = PerformFolderSubstitution(x.Value),
                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load MediaAccess configuration", ex);
            }
        }

        public bool Save()
        {
            try
            {
                XElement file = XElement.Load(Configuration.GetPath("MediaAccess.xml"));

                file.Element("plugins").Element("tvShow").Value = TVShowPlugin;
                file.Element("plugins").Element("movie").Value = MoviePlugin;
                file.Element("plugins").Element("music").Value = MusicPlugin;
                file.Element("plugins").Element("picture").Value = PicturePlugin;
                file.Element("plugins").Element("filesystem").Value = FilesystemPlugin;

                foreach (var pluginElement in file.Element("pluginConfiguration").Elements("plugin"))
                {
                    List<PluginConfigItem> newConfig = PluginConfiguration[pluginElement.Attribute("name").Value];
                    foreach (var configItem in pluginElement.Elements())
                    {
                        configItem.Value = newConfig.First(x => x.Name == configItem.Name.LocalName).Value;
                    }
                }

                file.Save(Configuration.GetPath("MediaAccess.xml"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to write MediaAccess.xml", ex);
                return false;
            }
        }

        private static string PerformFolderSubstitution(string input)
        {
            string cappdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            input = input.Replace("%ProgramData%", cappdata);
            return input;
        }
    }
}
