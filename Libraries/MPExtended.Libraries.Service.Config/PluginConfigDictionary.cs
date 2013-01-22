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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    public class PluginConfigDictionary : Dictionary<string, List<PluginConfigItem>>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XElement document = XElement.Load(reader.ReadSubtree(), LoadOptions.None);
            foreach (XElement plugin in document.Elements(XName.Get("Plugin", document.Name.NamespaceName)))
            {
                this[plugin.Attribute("name").Value] = new List<PluginConfigItem>();
                foreach (var configItem in plugin.Elements())
                {
                    ConfigType type = (ConfigType)Enum.Parse(typeof(ConfigType), configItem.Attribute("type").Value, true);
                    string value = type == ConfigType.File || type == ConfigType.Folder ? Transformations.FolderNames(configItem.Value) : configItem.Value;
                    this[plugin.Attribute("name").Value].Add(new PluginConfigItem()
                    {
                        DisplayName = configItem.Attribute("displayname").Value,
                        Name = configItem.Name.LocalName,
                        Type = type,
                        Value = value
                    });
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var item in this)
            {
                writer.WriteStartElement("Plugin");
                writer.WriteAttributeString("name", item.Key);
                foreach (var pci in item.Value)
                {
                    writer.WriteStartElement(pci.Name);
                    writer.WriteAttributeString("type", pci.Type.ToString());
                    writer.WriteAttributeString("displayname", pci.DisplayName);
                    writer.WriteValue(pci.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }
    }
}
