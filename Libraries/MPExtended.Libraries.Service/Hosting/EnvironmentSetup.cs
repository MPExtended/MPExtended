#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Hosting
{
    public static class EnvironmentSetup
    {
        public static void SetupEnvironment()
        {
            CallWithExceptionHandling(EnsurePermissionSettings, "setting permissions");
        }

        private static void CallWithExceptionHandling(Action action, string description)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("EnvironmentChecks: Error while {0}", description), ex);
            }
        }

        private static void EnsurePermissionSettings()
        {
            var rights = FileSystemRights.CreateDirectories | FileSystemRights.CreateFiles | FileSystemRights.ListDirectory | FileSystemRights.Modify | FileSystemRights.DeleteSubdirectoriesAndFiles;
            var everyone = FilePermissions.EveryoneIdentity;

            if (Installation.GetFileLayoutType() == FileLayoutType.Installed)
            {
                FilePermissions.SetAclForUserOnDirectory(Installation.GetConfigurationDirectory(), everyone, rights, AccessControlType.Allow);
                foreach (var file in Directory.GetFiles(Installation.GetConfigurationDirectory(), "*.xml"))
                    FilePermissions.SetAclForUserOnFile(file, everyone, rights, AccessControlType.Allow);
            }

            FilePermissions.SetAclForUserOnDirectory(Installation.GetCacheDirectory(), everyone, rights, AccessControlType.Allow);
            FilePermissions.SetAclForUserOnDirectory(Installation.GetLogDirectory(), everyone, rights, AccessControlType.Allow);
        }
    }
}
