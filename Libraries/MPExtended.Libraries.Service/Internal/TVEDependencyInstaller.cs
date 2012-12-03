#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Microsoft.Win32;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Internal
{
    internal static class TVEDependencyInstaller
    {
        public enum DependencyStatus
        {
            NoTVServiceAvailable,
            NotInstalledAsService,
            NoDependencySet,
            DependencySet,
            Failed
        }

        public static DependencyStatus EnsureDependencyStatus(DependencyStatus status)
        {
            try
            {
                if (status != DependencyStatus.NoDependencySet && status != DependencyStatus.DependencySet)
                    throw new ArgumentException("Can only ensure dependency (un-)set status");

                DependencyStatus currentStatus = GetCurrentStatus();
                DependencyStatus? actionResult = null;
                Log.Trace("TVEDependencyInstaller: Current status is {0}", currentStatus);
                if (currentStatus == DependencyStatus.NoDependencySet && status == DependencyStatus.DependencySet)
                    actionResult = InstallDependency();
                if (currentStatus == DependencyStatus.DependencySet && status == DependencyStatus.NoDependencySet)
                    actionResult = RemoveDependency();

                return actionResult ?? currentStatus;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("Failed to ensure TVEngine dependency status to {0}", status), ex);
                return DependencyStatus.Failed;
            }
        }

        private static DependencyStatus GetCurrentStatus()
        {
            // short-circuit when running from source or dependency already set
            if (Installation.GetFileLayoutType() == FileLayoutType.Source)
                return DependencyStatus.NotInstalledAsService;
            if ((string)RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"Software\MPExtended", "TVEDependencyInstalled") == "true")
                return DependencyStatus.DependencySet;
            if ((string)RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"Software\MPExtended", "TVEDependencyInstalled") == "false")
                return DependencyStatus.NoDependencySet;

            var services = ServiceController.GetServices();
            if (!services.Any(x => x.ServiceName == "MPExtended Service"))
                return DependencyStatus.NotInstalledAsService;
            if (!services.Any(x => x.ServiceName == "TVService"))
                return DependencyStatus.NoTVServiceAvailable;

            return DependencyStatus.NoDependencySet;
        }

        private static Process ExecuteScProcess(string arguments)
        {
            var info = new ProcessStartInfo();
            info.FileName = "sc";
            info.Arguments = arguments;
            info.CreateNoWindow = true;
            var proc = Process.Start(info);
            proc.WaitForExit();
            return proc;
        }

        private static DependencyStatus InstallDependency()
        {
            var currentStatus = GetCurrentStatus();
            if (currentStatus != DependencyStatus.NoDependencySet)
                return currentStatus;

            try
            {
                Log.Debug("TVEDependencyInstaller: Executing sc config to install dependency");
                ExecuteScProcess("config \"MPExtended Service\" depend= TVService");
                Log.Trace("TVEDependencyInstaller: Done!");

                RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\MPExtended", true);
                parentKey.SetValue("TVEDependencyInstalled", "true");
                parentKey.Close();

                return DependencyStatus.DependencySet;
            }
            catch (Exception ex)
            {
                Log.Warn("TVEDependencyInstaller: Failed to set TVService dependency", ex);
                return DependencyStatus.Failed;
            }
        }

        private static DependencyStatus RemoveDependency()
        {
            var currentStatus = GetCurrentStatus();
            if (currentStatus != DependencyStatus.DependencySet)
                return currentStatus;

            try
            {
                Log.Debug("TVEDependencyInstaller: Executing sc config to remove dependency");
                ExecuteScProcess("config \"MPExtended Service\" depend= /");
                Log.Trace("TVEDependencyInstaller: Done!");

                RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\MPExtended", true);
                parentKey.SetValue("TVEDependencyInstalled", "false");
                parentKey.Close();

                return DependencyStatus.NoDependencySet;
            }
            catch (Exception ex)
            {
                Log.Warn("TVEDependencyInstaller: Failed to remove TVService dependency", ex);
                return DependencyStatus.Failed;
            }
        }
    }
}