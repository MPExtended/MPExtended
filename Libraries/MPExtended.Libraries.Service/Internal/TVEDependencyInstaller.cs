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

        private static DependencyStatus GetCurrentStatus()
        {
            // short-circuit when running from source or dependency already set
            if (Installation.GetFileLayoutType() == FileLayoutType.Source)
                return DependencyStatus.NotInstalledAsService;
            if ((string)RegistryReader.ReadKeyAllViews(RegistryHive.LocalMachine, @"Software\MPExtended", "TVEDependencyInstalled") == "true")
                return DependencyStatus.DependencySet;

            var services = ServiceController.GetServices();
            if (!services.Any(x => x.ServiceName == "MPExtended Service"))
                return DependencyStatus.NotInstalledAsService;
            if (!services.Any(x => x.ServiceName == "TVService"))
                return DependencyStatus.NoTVServiceAvailable;

            return DependencyStatus.NoDependencySet;
        }

        private static DependencyStatus InstallDependency()
        {
            var currentStatus = GetCurrentStatus();
            if (currentStatus != DependencyStatus.NoDependencySet)
                return currentStatus;

            try
            {
                Log.Debug("TVEDependencyInstaller: Executing sc config");
                var info = new ProcessStartInfo();
                info.FileName = "sc";
                info.Arguments = "config \"MPExtended Service\" depend= TVService";
                info.CreateNoWindow = true;
                var proc = Process.Start(info);
                proc.WaitForExit();
                Log.Trace("TVEDependencyInstaller: Done!");
                return DependencyStatus.DependencySet;
            }
            catch (Exception ex)
            {
                Log.Warn("TVEDependencyInstaller: Failed to set TVService dependency", ex);
                return DependencyStatus.Failed;
            }
        }

        public static void EnsureDependencyIsInstalled()
        {
            try
            {
                var currentStatus = GetCurrentStatus();
                Log.Trace("TVEDependencyInstaller: Current status is {0}", currentStatus);
                if (currentStatus == DependencyStatus.NoDependencySet && InstallDependency() == DependencyStatus.DependencySet)
                {
                    RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\MPExtended", true);
                    parentKey.SetValue("TVEDependencyInstalled", "true");
                    parentKey.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Warn("TVEDependencyInstaller: Failed to ensure TVE dependency", ex);
            }
        }
    }
}