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
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabConfiguration.xaml
    /// </summary>
    public partial class TabConfiguration : Page
    {
        public TabConfiguration()
        {
            InitializeComponent();

            // load config
            txtPort.Text = Configuration.Services.Port.ToString();
            txtServiceName.Text = Configuration.Services.BonjourName.ToString();
            cbBonjourEnabled.IsChecked = Configuration.Services.BonjourEnabled;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Services.Port = Int32.Parse(txtPort.Text);
            Configuration.Services.BonjourName = txtServiceName.Text;
            Configuration.Services.BonjourEnabled = cbBonjourEnabled.IsChecked.Value;
            Configuration.Services.Save();
        }
    }
}
