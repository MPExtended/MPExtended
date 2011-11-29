#region Copyright (C) 2011 MPExtended
// Copyright (c) 2011 MPExtended Developers, http://mpextended.github.com/
//
// The use and distribution terms for this software are covered by the
// Common Public License 1.0 (http://opensource.org/licenses/cpl1.0.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by
// the terms of this license.
//    
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;

namespace MPExtended.Installers.CustomActions
{
    public static class Log
    {
        public static Session Session { private get; set; }

        private static TextWriter logFile;

        public static void Write(string message)
        {
            Session.Log(message);

            if (logFile == null)
            {
                // try to log into MPExtended log directory, but fallback to temporary directory
                string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs");
                if (!Directory.Exists(directory))
                {
                    Session.Log("Falling back to file in temporary directory for logging");
                    directory = Path.Combine(Path.GetTempPath(), "MPExtended");
                    Directory.CreateDirectory(directory);
                }

                // open file
                string logFilePath = Path.Combine(directory, "Installer.log");
                logFile = new StreamWriter(logFilePath, true);
                logFile.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.ffffff}] {1}", DateTime.Now, "Created installer logfile");
            }

            logFile.WriteLine("[{0:yyyy-MM-dd HH:mm:ss.ffffff}] {1}", DateTime.Now, message);
            logFile.Flush();
        }

        public static void Write(string message, params object[] parameters) 
        {
            string msg = String.Format(message, parameters);
            Write(msg);
        }
    }
}
