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
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabWebMediaPortal.xaml
    /// </summary>
    public partial class TabWebMediaPortal : Page, ITabCloseCallback
    {
        public TabWebMediaPortal()
        {
            InitializeComponent();

            txtPort.Text = Configuration.WebMediaPortalHosting.Port.ToString();
        }

        public void TabClosed()
        {
            Configuration.WebMediaPortalHosting.Port = Int32.Parse(txtPort.Text);
            Configuration.WebMediaPortalHosting.Save();

            // TODO: restart WebMP hosting process (#134)
        }
    }
}
