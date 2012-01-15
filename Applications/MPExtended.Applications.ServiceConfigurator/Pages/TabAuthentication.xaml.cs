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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.ConfigurationContracts;

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
            foreach (User u in Configuration.Services.Users)
            {
                users.Add(u);
            }

            lvUsers.DataContext = users;

            cbEnable.IsChecked = Configuration.Services.AuthenticationEnabled;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Services.Users = users.ToList();
            Configuration.Services.AuthenticationEnabled = cbEnable.IsChecked.GetValueOrDefault(true);
            if (Configuration.Services.Save())
            {
                string message = "Updated users and passwords. Please restart the service for the changes to take affect.";
                MessageBox.Show(message, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Save failed", "MPExtended", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            users.Remove((User)lvUsers.SelectedItem);
            lvUsers.DataContext = users;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            User u = new User() { Username = txtUsername.Text };
            u.SetPasswordFromPlaintext(txtPassword.Password);
            users.Add(u);
            txtUsername.Text = "";
            txtPassword.Password = "";
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnAdd.IsEnabled = !users.Any(x => x.Username == txtUsername.Text);
        }

        private void cbEnable_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool enable = cbEnable.IsChecked.HasValue && cbEnable.IsChecked.Value;
            btnAdd.IsEnabled = enable;
            txtUsername.IsEnabled = enable;
            txtPassword.IsEnabled = enable;
            lvUsers.IsEnabled = enable;
        }
    }
}
