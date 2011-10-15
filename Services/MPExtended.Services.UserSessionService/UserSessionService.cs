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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using Microsoft.Win32;
using MPExtended.Libraries.General;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.UserSessionService.Interfaces;

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
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal");
                var value = key.GetValue("InstallPath");
                MPPath = Path.Combine(value.ToString(), "MediaPortal.exe");
            }
            catch (Exception e)
            {
                Log.Error("Failed to read MP installation path from registry", e);
                MPPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Team MediaPortal", "MediaPortal", "MediaPortal.exe");
            }
        }

        public WebResult TestConnection()
        {
            return new WebResult(true);
        }

        public WebResult IsMediaPortalRunning()
        {
            return new WebResult(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MPPath)).Length > 0);
        }

        public WebResult StartMediaPortal()
        {
            LaunchMediaPortal();
            return new WebResult(true);
        }

        public WebResult StartMediaPortalBlocking()
        {
            if (!LaunchMediaPortal())
            {
                return new WebResult(false);
            }

            while (!IsMediaPortalRunning().Status)
            {
                System.Threading.Thread.Sleep(500);
            }

            return new WebResult(true);
        }

        private bool LaunchMediaPortal()
        {
            try
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
            catch (Exception ex)
            {
                Log.Warn("Failed to launch mediaportal", ex);
                return false;
            }
        }

        public WebResult SetPowerMode(WebPowerMode powerMode)
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

            return new WebResult(true);
        }

        private RestartOptions MapPowerMode(WebPowerMode powerMode)
        {
            switch (powerMode)
            {
                case WebPowerMode.LogOff:
                    return RestartOptions.LogOff;
                case WebPowerMode.PowerOff:
                    return RestartOptions.PowerOff;
                case WebPowerMode.Reboot:
                    return RestartOptions.Reboot;
                case WebPowerMode.ShutDown:
                    return RestartOptions.ShutDown;
                case WebPowerMode.Suspend:
                    return RestartOptions.Suspend;
                case WebPowerMode.Hibernate:
                    return RestartOptions.Hibernate;
                default:
                    throw new InvalidCastException();
            }
        }

        public WebResult CloseMediaPortal()
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MPPath));

            if (processes != null && processes.Length == 1)
            {
                processes[0].CloseMainWindow();
            }

            return new WebResult(true);
        }
    }
}
