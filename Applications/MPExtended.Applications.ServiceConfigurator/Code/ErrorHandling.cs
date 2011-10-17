using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Libraries.General;
using System.Windows.Forms;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal static class ErrorHandling
    {
        public static void ShowError(Exception ex)
        {
            Log.Warn("Unexpected error happened", ex);
            MessageBox.Show("An unexpected error occured. Please file a bugreport with the service's log files attached", ex.Message);
        }
    }
}
