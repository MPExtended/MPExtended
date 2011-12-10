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
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class Settings
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
            SettingModel loadedObj = null;
 
            try
            {
                // load from file using XmlSerializer
                XmlSerializer SerializerObj = new XmlSerializer(typeof(SettingModel));
                FileStream ReadFileStream = new FileStream(Configuration.GetPath("WebMediaPortal.xml"), FileMode.Open, FileAccess.Read, FileShare.Read);
                loadedObj = (SettingModel)SerializerObj.Deserialize(ReadFileStream);

                // cleanup
                ReadFileStream.Close();
                ReadFileStream.Dispose();
                return loadedObj;
             
            }
            catch (Exception ex)
            {
                Log.Debug("Exception in LoadSettings", ex);
            }

            // set some default values
            loadedObj.MASUrl = "auto://127.0.0.1:4322";
            loadedObj.TASUrl = "auto://127.0.0.1:4322";
            return loadedObj;
        }

        private static void SaveSettings(SettingModel settings)
        {
            // Create a new XmlSerializer instance with the type of the test class
            XmlSerializer SerializerObj = new XmlSerializer(typeof(SettingModel));

            // Create a new file stream to write the serialized object to a file
            TextWriter WriteFileStream = new StreamWriter(Configuration.GetPath("WebMediaPortal.xml"));
            SerializerObj.Serialize(WriteFileStream, settings);

            // Cleanup
            WriteFileStream.Close();
            WriteFileStream.Dispose();
        }
    }
}