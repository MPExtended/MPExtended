using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    public class UacServiceHelper
    {
        public static bool StartService()
        {
            return RunUacServiceHandler("start");
        }

        public static bool StopService()
        {
            return RunUacServiceHandler("stop");
        }

        public static bool RestartService()
        {
            return RunUacServiceHandler("restart");
        }

        private static bool RunUacServiceHandler(String _parameters)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "MPExtended.Applications.UacServiceHandler.exe";
                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                info.Arguments = _parameters;

                if (Process.Start(info) == null)
                {
                    // The user didn't accept the UAC prompt.
                    MessageBox.Show("This action needs administrative rights");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error starting uac service handler", ex);
                return false;
            }
            return true;
        }
    }
}
