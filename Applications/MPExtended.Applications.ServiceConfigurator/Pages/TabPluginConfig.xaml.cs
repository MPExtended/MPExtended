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
using System.Windows.Controls;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabPluginConfig.xaml
    /// </summary>
    public partial class TabPluginConfig : Page
    {
        private Dictionary<string, List<PluginConfigItem>> pluginConfigurations;

        public TabPluginConfig()
        {
            InitializeComponent();

            pluginConfigurations = Configuration.Media.PluginConfiguration;

            cbPluginConfigs.DataContext = pluginConfigurations;
            cbPluginConfigs.DisplayMemberPath = "Key";

            if (pluginConfigurations.Count > 0)
            {
                cbPluginConfigs.SelectedIndex = 0;
            }
        }

        private void cbPluginConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValuePair<string, List<PluginConfigItem>> item = (KeyValuePair<string, List<PluginConfigItem>>)cbPluginConfigs.SelectedValue;
            sectionPluginSettings.SetPluginConfig(item.Key, item.Value);
        }
    }
}
