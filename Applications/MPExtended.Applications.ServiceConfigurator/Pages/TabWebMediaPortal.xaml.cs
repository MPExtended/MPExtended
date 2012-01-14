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
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.ServiceConfigurator.Pages
{
    /// <summary>
    /// Interaction logic for TabWebMediaPortal.xaml
    /// </summary>
    public partial class TabWebMediaPortal : Page
    {
        public TabWebMediaPortal()
        {
            InitializeComponent();

            txtPort.Text = Configuration.WebMediaPortalHosting.Port.ToString();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Configuration.WebMediaPortalHosting.Port = Int32.Parse(txtPort.Text);
            Configuration.WebMediaPortalHosting.Save();
        }
    }
}
