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
using System.Collections.ObjectModel;
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
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabAuthentication.xaml
    /// </summary>
    public partial class TabAuthentication : Page
    {
        private ObservableCollection<User> users = new ObservableCollection<User>();

        public TabAuthentication()
        {
            InitializeComponent();
            foreach(User u in Configuration.Services.Users) 
            {
                users.Add(u);
            }

            lvUsers.DataContext = users;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Services.Users = users.ToList();
            if (Configuration.Services.Save())
            {
                MessageBox.Show("Updated users and passwords. Please restart the service for the changes to take effect.");
            }
            else
            {
                MessageBox.Show("Save failed");
            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            users.Remove((User)lvUsers.SelectedItem);
            lvUsers.DataContext = users;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            users.Add(new User() { Username = txtUsername.Text, Password = txtPassword.Password });
            txtUsername.Text = "";
            txtPassword.Password = "";
        }
    }
}
