using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MPExtended.Applications.WebMediaPortal.Models;
using System.Xml.Serialization;
using System.IO;
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