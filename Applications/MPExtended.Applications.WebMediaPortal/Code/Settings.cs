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
using System.Web;
using System.Xml;
using System.Text;
using System.Runtime.Serialization;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    internal static class Settings
    {
        public static SettingModel ActiveSettings
        {
            get
            {
                return LoadSettings();
            }

            set
            {
                SaveSettings(value);
            }
        }

        public static SettingModel LoadSettings()
        {
            // default values
            SettingModel settings = new SettingModel();
            settings.MASUrl = "auto://127.0.0.1:4322";
            settings.TASUrl = "auto://127.0.0.1:4322";
 
            try
            {
                using(XmlReader reader = XmlReader.Create(Configuration.GetPath("WebMediaPortal.xml")))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(SettingModel));
                    settings = (SettingModel)serializer.ReadObject(reader);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Exception in LoadSettings", ex);
            }

            return settings;
        }

        private static void SaveSettings(SettingModel settings)
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.CloseOutput = true;
            writerSettings.Indent = true;
            writerSettings.OmitXmlDeclaration = false;
            using (XmlWriter writer = XmlWriter.Create(Configuration.GetPath("WebMediaPortal.xml"), writerSettings))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(SettingModel));
                serializer.WriteObject(writer, settings);
            }
        }
    }
}