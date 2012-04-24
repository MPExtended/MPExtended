#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.Common.Interfaces;
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool LockWorkStation();

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MONITORPOWER = 0xF170;
        private const int HWND_BROADCAST = 0xFFFF;

        private string MPPath;

        public UserSessionService()
        {
            var mpdir = Mediaportal.GetClientInstallationDirectory();
            if (mpdir == null)
            {
                mpdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Team MediaPortal", "MediaPortal");
            }
            MPPath = Path.Combine(mpdir, "MediaPortal.exe");
        }

        public WebBoolResult TestConnection()
        {
            return new WebBoolResult(true);
        }

        public WebBoolResult IsMediaPortalRunning()
        {
            return new WebBoolResult(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MPPath)).Length > 0);
        }

        public WebBoolResult StartMediaPortal()
        {
            LaunchMediaPortal();
            return new WebBoolResult(true);
        }

        public WebBoolResult StartMediaPortalBlocking()
        {
            if (!LaunchMediaPortal())
            {
                return new WebBoolResult(false);
            }

            while (!IsMediaPortalRunning().Result)
            {
                System.Threading.Thread.Sleep(500);
            }

            return new WebBoolResult(true);
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
                SetForegroundWindow(proc.MainWindowHandle);

                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to launch mediaportal", ex);
                return false;
            }
        }

        public WebBoolResult SetMediaPortalForeground()
        {
            try
            {
                Log.Info("Bringing MediaPortal to the front");
                Process[] processes = Process.GetProcessesByName("MediaPortal");

                if (processes.Length == 1)
                {
                    SetForegroundWindow(processes[0].MainWindowHandle);
                    return true;
                }                
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to bring MediaPortal to the front", ex);
            }
            return false;
        }

        public WebBoolResult SetPowerMode(WebPowerMode powerMode)
        {
            try
            {
                if (powerMode == WebPowerMode.Screensaver)
                {
                    ScreenSaver.StartScreenSaver();
                }
                else if (powerMode == WebPowerMode.Lock)
                {
                    LockWorkStation();
                }
                else if (powerMode == WebPowerMode.ScreenOff)
                {
                    SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, 2);
                }
                else if (powerMode == WebPowerMode.ScreenOn)
                {

                }
                else
                {
                    RestartOptions option = MapPowerMode(powerMode);
                    WindowsController.ExitWindows(option, false);
                }
            }
            catch (InvalidCastException)
            {
                Log.Warn("The powerMode " + powerMode + " is not valid");
            }

            return new WebBoolResult(true);
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

        public WebBoolResult CloseMediaPortal()
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MPPath));
            if (processes != null && processes.Length == 1)
            {
                processes[0].CloseMainWindow();
            }

            return new WebBoolResult(true);
        }
    }
}
