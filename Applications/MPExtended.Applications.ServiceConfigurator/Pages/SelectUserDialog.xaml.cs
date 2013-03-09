#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for SelectUserDialog.xaml
    /// </summary>
    public partial class SelectUserDialog : Window
    {
        public String SelectedUser { get; set; }
        public bool UserHasResponded { get; set; }

        public SelectUserDialog()
        {
            InitializeComponent();
            this.UserHasResponded = false;
        }

        public SelectUserDialog(string title, string text, List<string> users): this()
        {
            this.Title = title;
            textBlockDialogText.Text = text;
            foreach (string u in users)
            {
                comboBoxUsers.Items.Add(u);
            }
            comboBoxUsers.SelectedIndex = 0;
        }

        private void comboBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedUser = (string)comboBoxUsers.SelectedItem;
        }

        private void buttonGrantAccess_Click(object sender, RoutedEventArgs e)
        {
            this.UserHasResponded = true;
            Close();
        }

        private void buttonDenyAccess_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedUser = null;
            this.UserHasResponded = true;
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.UserHasResponded = true;
        }
    }
}
