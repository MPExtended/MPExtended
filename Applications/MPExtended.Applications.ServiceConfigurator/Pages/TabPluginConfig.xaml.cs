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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabPluginConfig.xaml
    /// </summary>
    public partial class TabPluginConfig : Page
    {
        private Dictionary<String, Dictionary<String, PluginConfigItem>> PluginConfigurations { get; set; }
        private XElement mConfigFile;

        public TabPluginConfig()
        {
            InitializeComponent();
            PluginConfigurations = new Dictionary<String, Dictionary<String, PluginConfigItem>>();
            try
            {
                mConfigFile = XElement.Load(Configuration.GetPath("MediaAccess.xml"));
                var config = mConfigFile.Element("pluginConfiguration").Elements("plugin");

                foreach (var p in config)
                {
                    Dictionary<String, PluginConfigItem> props = new Dictionary<String, PluginConfigItem>();
                    foreach (var p2 in p.Descendants())
                    {
                        props.Add(p2.Name.LocalName, new PluginConfigItem(p2) { Tag = p2 });
                        //.ToDictionary(x => x.Key, x => PerformFolderSubstitution(x.Value));
                    }
                    PluginConfigurations.Add(p.Attribute("name").Value, props);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading plugin config");
                Log.Error(ex.ToString());
            }

            cbPluginConfigs.DataContext = PluginConfigurations;
            cbPluginConfigs.DisplayMemberPath = "Key";
            cbPluginConfigs.SelectedValuePath = "Value";

            if (PluginConfigurations.Count > 0)
            {
                cbPluginConfigs.SelectedIndex = 0;
            }
        }

        private void cbPluginConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<String, PluginConfigItem> selected = (Dictionary<String, PluginConfigItem>)cbPluginConfigs.SelectedValue;
            sectionPluginSettings.SetPluginConfig(XElement.Load(Configuration.GetPath("MediaAccess.xml")), selected);
        }
    }
}
