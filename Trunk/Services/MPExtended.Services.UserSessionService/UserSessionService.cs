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
using System.Runtime.InteropServices;

namespace MPExtended.Services.UserSessionService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class UserSessionService : IUserSessionService
    {     
        /// <summary>
        /// The SetSuspendState function suspends the system by shutting power down. Depending on the Hibernate parameter, the system either enters a suspend (sleep) state or hibernation (S4). If the ForceFlag parameter is TRUE, the system suspends operation immediately; if it is FALSE, the system requests permission from all applications and device drivers before doing so.
        /// </summary>
        /// <param name="Hibernate">Specifies the state of the system. If TRUE, the system hibernates. If FALSE, the system is suspended.</param>
        /// <param name="ForceCritical">Forced suspension. If TRUE, the function broadcasts a PBT_APMSUSPEND event to each application and driver, then immediately suspends operation. If FALSE, the function broadcasts a PBT_APMQUERYSUSPEND event to each application to request permission to suspend operation.</param>
        /// <param name="DisableWakeEvent">If TRUE, the system disables all wake events. If FALSE, any system wake events remain enabled.</param>
        /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
        [DllImport("powrprof.dll", EntryPoint = "SetSuspendState", CharSet = CharSet.Ansi)]
        private static extern int SetSuspendState(int Hibernate, int ForceCritical, int DisableWakeEvent);

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

        public void SetPowerMode(WebPowerModes powerMode)
        {
            try
            {
                RestartOptions option = MapPowerMode(powerMode);
                WindowsController.ExitWindows(option, false);
            }
            catch (InvalidCastException)
            {
                Log.Warn("The powerMode " + powerMode + " is not valid");
            }

           //PowerState.
        }

        private RestartOptions MapPowerMode(WebPowerModes powerMode)
        {
            switch (powerMode)
            {
                case WebPowerModes.LogOff:
                    return RestartOptions.LogOff;
                case WebPowerModes.PowerOff:
                    return RestartOptions.PowerOff;
                case WebPowerModes.Reboot:
                    return RestartOptions.Reboot;
                case WebPowerModes.ShutDown:
                    return RestartOptions.ShutDown;
                case WebPowerModes.Suspend:
                    return RestartOptions.Suspend;
                case WebPowerModes.Hibernate:
                    return RestartOptions.Hibernate;
                default:
                    throw new InvalidCastException();
            }
        }

        public void CloseMediaPortal()
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MPPath));

            if (processes != null && processes.Length == 1)
            {
                processes[0].CloseMainWindow();
            }
        }
    }
}
