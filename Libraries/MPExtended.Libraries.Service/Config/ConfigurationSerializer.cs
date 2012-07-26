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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace MPExtended.Libraries.Service.Config
{
    internal class ConfigurationSerializer<TModel>
    {
        public string Filename { get; private set; }

        private TModel _instance;

        public ConfigurationSerializer(string filename)
        {
            this.Filename = filename;
        }

        public TModel Load()
        {
            string path = Configuration.GetPath(Filename);
            using (XmlReader reader = XmlReader.Create(path))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(TModel));
                return (TModel)serializer.ReadObject(reader);
            }
        }

        public TModel Get()
        {
            if (_instance == null)
                _instance = Load();

            return _instance;
        }

        public bool Save()
        {
            try
            {
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.CloseOutput = true;
                writerSettings.Indent = true;
                writerSettings.OmitXmlDeclaration = false;
                using (XmlWriter writer = XmlWriter.Create(Filename, writerSettings))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(TModel));
                    serializer.WriteObject(writer, _instance);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to save settings to file {0}", Filename), ex);
                return false;
            }
        }
    }
}
