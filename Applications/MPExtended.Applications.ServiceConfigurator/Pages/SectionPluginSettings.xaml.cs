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
using System.Windows.Documents;
using MPExtended.Libraries.General;
using MPExtended.Applications.ServiceConfigurator.Code;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for SectionPluginSettings.xaml
    /// </summary>
    public partial class SectionPluginSettings : UserControl
    {
        private Dictionary<string, PluginConfigItem> mConfiguration;
        private List<Control> mControls;
        private string mPlugin;

        public SectionPluginSettings()
        {
            InitializeComponent();
        }

        public void SetPluginConfig(string plugin, List<PluginConfigItem> configuration)
        {
            mPlugin = plugin;
            mConfiguration = configuration.ToDictionary(x => x.Name, x => x);
            mControls = new List<Control>();

            ConfigurationItems.Children.Clear();
            int rowHeight = 10;

            foreach (PluginConfigItem item in configuration)
            {               
                Label text = new Label();
                text.Margin = new Thickness(10, rowHeight - 2, 0, 0);
                text.VerticalAlignment = VerticalAlignment.Top;
                text.HorizontalAlignment = HorizontalAlignment.Left;
                text.Content = item.DisplayName;
                text.FontWeight = FontWeights.Bold;
                ConfigurationItems.Children.Add(text);

                switch (item.Type)
                {
                    case ConfigType.File:
                        CreateFileChooser(rowHeight, item);
                        break;
                    case ConfigType.Folder:
                        CreateFolderSelect(rowHeight, item);
                        break;
                    case ConfigType.Number:
                    case ConfigType.Text:
                        CreateTextBox(rowHeight, item);
                        break;
                    case ConfigType.Boolean:
                        CreateBoolean(rowHeight, item);
                        break;
                }
                
                rowHeight += 30;
            }
        }

        private void CreateBoolean(int rowHeight, PluginConfigItem item)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Margin = new Thickness(130, rowHeight + 5, 0, 0);
            checkBox.VerticalAlignment = VerticalAlignment.Top;
            checkBox.HorizontalAlignment = HorizontalAlignment.Left;
            checkBox.IsChecked = Boolean.Parse(item.Value);
            checkBox.Tag = item.Name;
            ConfigurationItems.Children.Add(checkBox);
            mControls.Add(checkBox);
        }

        private void CreateFolderSelect(int rowHeight, PluginConfigItem item)
        {
            TextBox textbox = new TextBox();
            textbox.Margin = new Thickness(130, rowHeight, 110, 0);
            textbox.Text = item.Value;
            textbox.VerticalAlignment = VerticalAlignment.Top;
            textbox.Tag = item.Name;
            ConfigurationItems.Children.Add(textbox);
            mControls.Add(textbox);

            Button btnSelectFolder = new Button();
            btnSelectFolder.Click += new RoutedEventHandler(btnSelectFolder_Click);
            btnSelectFolder.Content = "Browse";
            btnSelectFolder.VerticalAlignment = VerticalAlignment.Top;
            btnSelectFolder.HorizontalAlignment = HorizontalAlignment.Right;
            btnSelectFolder.Margin = new Thickness(0, rowHeight, 10, 0);
            btnSelectFolder.Width = 75;
            btnSelectFolder.Tag = textbox;
            ConfigurationItems.Children.Add(btnSelectFolder);
        }

        void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            Button o = (Button)sender;

            // Create OpenFileDialog
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.RootFolder = Environment.SpecialFolder.Desktop;
            dlg.SelectedPath = ((TextBox)o.Tag).Text;

            // Display OpenFileDialog by calling ShowDialog method
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Open document
                string foldername = dlg.SelectedPath;
                ((TextBox)o.Tag).Text = foldername;
            }
        }

        private void CreateTextBox(int rowHeight, PluginConfigItem item)
        {
            TextBox textbox = new TextBox();
            textbox.Margin = new Thickness(130, rowHeight, 10, 0);
            textbox.Text = item.Value;
            textbox.VerticalAlignment = VerticalAlignment.Top;
            textbox.Tag = item.Name;
            ConfigurationItems.Children.Add(textbox);
            mControls.Add(textbox);
        }

        private void CreateFileChooser(int rowHeight, PluginConfigItem item)
        {
            TextBox textbox = new TextBox();
            textbox.Margin = new Thickness(130, rowHeight, 110, 0);
            textbox.Text = item.Value;
            textbox.VerticalAlignment = VerticalAlignment.Top;
            textbox.Tag = item.Name;
            ConfigurationItems.Children.Add(textbox);
            mControls.Add(textbox);

            Button btnSelectFile = new Button();
            btnSelectFile.Click += new RoutedEventHandler(btnSelectFile_Click);
            btnSelectFile.Content = "Select";
            btnSelectFile.VerticalAlignment = VerticalAlignment.Top;
            btnSelectFile.HorizontalAlignment = HorizontalAlignment.Right;
            btnSelectFile.Margin = new Thickness(0, rowHeight, 10, 0);
            btnSelectFile.Width = 75;
            btnSelectFile.Tag = textbox;
            ConfigurationItems.Children.Add(btnSelectFile);
        }

        void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            Button o = (Button)sender;

            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = ((TextBox)o.Tag).Text;
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                ((TextBox)o.Tag).Text = filename;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<PluginConfigItem> newConfig = new List<PluginConfigItem>();

                // save things here
                foreach (Control c in mControls)
                {
                    string value = "";
                    if (c is TextBox)
                    {
                        value = ((TextBox)c).Text;
                    }
                    else if (c is CheckBox)
                    {
                        value = ((CheckBox)c).IsChecked == true ? "true" : "false";
                    }
                    else
                    {
                        continue;
                    }

                    newConfig.Add(new PluginConfigItem(mConfiguration[(string)c.Tag])
                    {
                        Value = value
                    });
                }

                Configuration.Media.PluginConfiguration[mPlugin] = newConfig;
                if (Configuration.Media.Save())
                {
                    MessageBox.Show("Successfully updated config, please restart service for the changes to take affect.");
                }
                else
                {
                    MessageBox.Show("Failed to update config!");
                }
            }
            catch (Exception ex)
            {
                ErrorHandling.ShowError(ex);
            }
        }
    }
}
