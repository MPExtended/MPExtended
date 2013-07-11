#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://mpextended.github.com/
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
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using MPExtended.Libraries.Service;

namespace MPExtended.ServiceHosts.WebMediaPortal
{
    internal static class EnvironmentSetup
    {
        public static void SetupAspDotNet()
        {
            // On Windows 8, the Temporary ASP.NET Files directory isn't created during installation of the .NET framework, so we
            // need to create it ourselves. Using another temporary directory is problematic because:
            // (a) It can only be set in the <compilation> attribute in Web.config, but we don't want to change it globally in
            //     WebMediaPortal (the current value should be kept for IIS setups).
            // (b) Changing the directory causes IIS Express to restart the application with every request, which is really bad
            //     for performance.
            var tempDir = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Temporary ASP.NET Files");
            if (!Directory.Exists(tempDir))
            {
                Log.Info("Creating ASP.NET temporary directory '{0}'", tempDir);
                Directory.CreateDirectory(tempDir);
            }

            // Assume that the directory is already writable when we can list all subdirectories, so nothing to do in that case.
            try
            {
                Directory.GetDirectories(tempDir);
                return;
            }
            catch (Exception)
            {
            }

            // Otherwise, set suitable permissions
            try
            {
                Log.Info("Going to give the current user write access on ASP.NET temporary directory '{0}'", tempDir);
                var rights = FileSystemRights.CreateDirectories | FileSystemRights.CreateFiles | FileSystemRights.ListDirectory |
                    FileSystemRights.Modify | FileSystemRights.DeleteSubdirectoriesAndFiles;
                var currentUser = WindowsIdentity.GetCurrent().User;
                AddAclRuleOnDirectory(tempDir, currentUser, rights, AccessControlType.Allow);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set permissions on ASP.NET temporary directory", ex);
            }
        }

        // FIXME: Copied from Libraries/MPExtended.Libraries.Service/Hosting/EnvironmentSetup.cs
        private static void AddAclRuleOnDirectory(string directory, IdentityReference identity, FileSystemRights rights, AccessControlType type)
        {
            var acl = Directory.GetAccessControl(directory);
            acl.PurgeAccessRules(identity);
            acl.AddAccessRule(new FileSystemAccessRule(identity, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, type));
            Directory.SetAccessControl(directory, acl);
        }
    }
}
