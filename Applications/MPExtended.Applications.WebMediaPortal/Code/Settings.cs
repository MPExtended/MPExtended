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
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class Settings
    {
        private static SettingModel activeSettingModel;

        public static SettingModel ActiveSettings
        {
            get
            {
                if (activeSettingModel == null)
                    LoadSettings();
                return activeSettingModel;
            }

            set
            {
                SaveSettings(value);
                LoadSettings();
            }
        }

        public static void LoadSettings()
        {
            activeSettingModel = PerformLoadSettings(true);
            ReloadSkinSettings();
        }

        private static SettingModel PerformLoadSettings(bool retry)
        {
            try
            {
                using(XmlReader reader = XmlReader.Create(Configuration.GetPath("WebMediaPortal.xml")))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(SettingModel));
                    return (SettingModel)serializer.ReadObject(reader);
                }
            }
            catch (Exception ex)
            {
                if (retry)
                {
                    Log.Warn("Exception in LoadSettings (due to old configuration file?), overwriting with default file and retrying", ex);
                    File.Copy(Configuration.GetDefaultPath("WebMediaPortal.xml"), Configuration.GetPath("WebMediaPortal.xml"), true);
                    return PerformLoadSettings(false);
                }
                else
                {
                    Log.Warn("Exception in LoadSettings, bailing out", ex);
                }
            }

            // default values
            SettingModel settings = new SettingModel();
            settings.MASUrl = "auto://127.0.0.1:4322";
            settings.TASUrl = "auto://127.0.0.1:4322";
            return settings;
        }

        private static void ReloadSkinSettings()
        {
            // Setup everything that uses settings from the current skin
            ContentLocator.Current.ChangeSkin(ActiveSettings.Skin);
            ControllerBuilder.Current.SetControllerFactory(new MEFControllerFactory(new HttpContextWrapper(HttpContext.Current)));
            foreach (var engine in ViewEngines.Engines.OfType<SkinnableViewEngine>())
            {
                engine.UpdateActiveSkin();
            }
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