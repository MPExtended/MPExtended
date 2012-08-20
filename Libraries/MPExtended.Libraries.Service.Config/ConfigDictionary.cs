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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    public class ConfigDictionary : Dictionary<string, string>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlReader stReader = reader.ReadSubtree();
            stReader.Read(); // read the start of the element
            string lastName = null;
            while (stReader.Read())
            {
                if (stReader.NodeType == XmlNodeType.Element)
                {
                    lastName = ReadKey(stReader);
                    this[lastName] = String.Empty; // for nodes without a value, use an empty string as value
                }
                else if (stReader.NodeType == XmlNodeType.Text)
                {
                    this[lastName] = ReadValue(stReader);
                }
            }

            if (reader.IsEmptyElement) // for empty nodes, we still need to read the content, as the subtree reader is empty
                reader.Read();

            reader.ReadEndElement(); // read the end of the element
        }

        protected virtual string ReadValue(XmlReader reader)
        {
            return reader.Value;
        }

        protected virtual string ReadKey(XmlReader reader)
        {
            return reader.Name;
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var item in this)
            {
                writer.WriteElementString(item.Key, item.Value);
            }
        }
    }
}
