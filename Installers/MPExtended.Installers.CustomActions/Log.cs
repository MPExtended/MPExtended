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
