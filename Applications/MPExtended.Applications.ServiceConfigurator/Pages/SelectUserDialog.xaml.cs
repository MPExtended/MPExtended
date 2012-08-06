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
