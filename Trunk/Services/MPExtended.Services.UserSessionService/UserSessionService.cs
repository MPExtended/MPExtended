#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.ServiceModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Services.UserSessionService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class UserSessionService : IUserSessionService
    {
        private string MPPath;

        public UserSessionService()
        {
            // read config file
            MPPath =
                XElement.Load(Configuration.GetPath("UserSessionService.xml"))
                .Element("mediaportal")
                .Value
                .Replace("%ProgramFiles%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
        }

        public bool IsMediaPortalRunning()
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MPPath)).Length > 0;
        }

        public bool StartMediaPortal()
        {
            return LaunchMediaPortal();
        }

        public bool StartMediaPortalBlocking()
        {
            if (!LaunchMediaPortal())
            {
                return false;
            }

            while (!IsMediaPortalRunning())
            {
                System.Threading.Thread.Sleep(500);
            }

            return true;
        }

        private bool LaunchMediaPortal()
        {
            Log.Info("Starting MediaPortal");

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = MPPath;
            info.CreateNoWindow = false;
            info.ErrorDialog = true;

            Process proc = new Process();
            proc.StartInfo = info;
            proc.Start();

            return true;
        }
    }
}
