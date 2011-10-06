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
using MPExtended.Services.MediaAccessService;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for SectionPluginSettings.xaml
    /// </summary>
    public partial class SectionPluginSettings : UserControl
    {
        public SectionPluginSettings()
        {
            InitializeComponent();
        }

        public void SetPluginConfig(Dictionary<String, PluginConfigItem> configuration)
        {
            ConfigurationItems.Children.Clear();
            int rowHeight = 10;
            foreach (KeyValuePair<String, PluginConfigItem> kvp in configuration)
            {               
                Label text = new Label();
                text.Margin = new Thickness(10, rowHeight - 2, 0, 0);
                text.VerticalAlignment = VerticalAlignment.Top;
                text.HorizontalAlignment = HorizontalAlignment.Left;
                text.Content = kvp.Value.DisplayName;
                text.FontWeight = FontWeights.Bold;
                ConfigurationItems.Children.Add(text);

                switch (kvp.Value.ConfigType)
                {
                    case ConfigType.File:
                        CreateFileChooser(rowHeight, kvp);
                        break;
                    case ConfigType.Folder:
                        CreateFolderSelect(rowHeight, kvp);
                        break;
                    case ConfigType.Text:
                        CreateTextBox(rowHeight, kvp);
                        break;
                    case ConfigType.Boolean:
                        CreateBoolean(rowHeight, kvp);
                        break;
                }
                
                rowHeight += 30;
            }
        }

        private void CreateBoolean(int rowHeight, KeyValuePair<string, PluginConfigItem> kvp)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Click += new RoutedEventHandler(checkBox_Click);
            checkBox.Margin = new Thickness(130, rowHeight + 5, 0, 0);
            checkBox.VerticalAlignment = VerticalAlignment.Top;
            checkBox.HorizontalAlignment = HorizontalAlignment.Left;
            //checkBox.Tag = textbox;
            ConfigurationItems.Children.Add(checkBox);
        }

        void checkBox_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void CreateFolderSelect(int rowHeight, KeyValuePair<string, PluginConfigItem> kvp)
        {
            TextBox textbox = new TextBox();
            textbox.Margin = new Thickness(130, rowHeight, 110, 0);
            textbox.Text = kvp.Value.ConfigValue;
            textbox.VerticalAlignment = VerticalAlignment.Top;
            ConfigurationItems.Children.Add(textbox);

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

        private void CreateTextBox(int rowHeight, KeyValuePair<string, PluginConfigItem> kvp)
        {
            TextBox textbox = new TextBox();
            textbox.Margin = new Thickness(130, rowHeight, 10, 0);
            textbox.Text = kvp.Value.ConfigValue;
            textbox.VerticalAlignment = VerticalAlignment.Top;
            ConfigurationItems.Children.Add(textbox);
        }

        private void CreateFileChooser(int rowHeight, KeyValuePair<string, PluginConfigItem> kvp)
        {
            TextBox textbox = new TextBox();
            textbox.Margin = new Thickness(130, rowHeight, 110, 0);
            textbox.Text = kvp.Value.ConfigValue;
            textbox.VerticalAlignment = VerticalAlignment.Top;
            ConfigurationItems.Children.Add(textbox);

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
    }
}
