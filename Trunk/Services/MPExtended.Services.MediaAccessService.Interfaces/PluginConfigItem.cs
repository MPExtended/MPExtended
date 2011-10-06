using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService
{
    /// <summary>
    /// Available config types
    /// file: Path to a file (e.g. database)
    /// folder: Path to a folder (e.g. fanart folder)
    /// text: Plain text (e.g. description)
    /// number: Integer text (e.g. Version)
    /// boolean: true or false 
    /// </summary>
    public enum ConfigType { File, Folder, Text, Number, Boolean }

    public class PluginConfigItem
    {
        private System.Xml.Linq.XElement n;

        public PluginConfigItem()
        {
            ConfigType = MediaAccessService.ConfigType.Text;
        }

        public PluginConfigItem(String value, String type)
        {
            ConfigValue = value;

            SetType(type);
        }

        private void SetType(string type)
        {
            if (type != null)
            {
                if (type.Equals("file"))
                {
                    ConfigType = MediaAccessService.ConfigType.File;
                }
                else if (type.Equals("folder"))
                {
                    ConfigType = MediaAccessService.ConfigType.Folder;
                }
                else if (type.Equals("text"))
                {
                    ConfigType = MediaAccessService.ConfigType.Text;
                }
                else if (type.Equals("number"))
                {
                    ConfigType = MediaAccessService.ConfigType.Number;
                }
                else if (type.Equals("boolean"))
                {
                    ConfigType = MediaAccessService.ConfigType.Boolean;
                }
            }
        }

        public PluginConfigItem(System.Xml.Linq.XElement n)
        {
            this.ConfigValue = n.Value;
            SetType((String)n.Attribute("type"));
            DisplayName = (String)n.Attribute("displayname");
            this.n = n;
        }


        /// <summary>
        /// Value of the config item
        /// </summary>
        public String ConfigValue { get; set; }

        /// <summary>
        /// Display name of this config item
        /// </summary>
        public String DisplayName { get; set; }

        /// <summary>
        /// Type of the config item
        /// </summary>
        public ConfigType ConfigType { get; set; }
    }
}
