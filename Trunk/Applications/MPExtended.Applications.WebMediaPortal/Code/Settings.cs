#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Services;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class Settings
    {
        public static SettingModel GlobalSettings
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
                XmlSerializer SerializerObj = new XmlSerializer(typeof(SettingModel));

                // Create a new file stream for reading the XML file
                FileStream ReadFileStream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\WebMediaPortal\Settings.xml", FileMode.Open, FileAccess.Read, FileShare.Read);

                // Load the object saved above by using the Deserialize function
                loadedObj = (SettingModel)SerializerObj.Deserialize(ReadFileStream);

                // Cleanup
                ReadFileStream.Close();
                ReadFileStream.Dispose();
             
            }
            catch (Exception ex)
            {
                Log.Debug("Exception in LoadSettings", ex);
            }
            if (loadedObj == null)
            {
                loadedObj = new SettingModel();
                loadedObj.DefaultGroup = 1;
                loadedObj.TranscodingProfile = "Flash HQ";
            }
            return loadedObj;
        }

        private static void SaveSettings(SettingModel settings)
        {
            // Create a new XmlSerializer instance with the type of the test class
            XmlSerializer SerializerObj = new XmlSerializer(typeof(SettingModel));

            // Create a new file stream to write the serialized object to a file
            TextWriter WriteFileStream = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\WebMediaPortal\Settings.xml");
            SerializerObj.Serialize(WriteFileStream, settings);

            // Cleanup
            WriteFileStream.Close();
            WriteFileStream.Dispose();
        }
    }
}