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
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

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
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            if (Installation.GetFileLayoutType() == FileLayoutType.Installed)
            {
                AddAclRuleOnDirectory(Installation.GetConfigurationDirectory(), everyone, rights, AccessControlType.Allow);
                foreach (var file in Directory.GetFiles(Installation.GetConfigurationDirectory(), "*.xml"))
                    AddAclRuleOnFile(file, everyone, rights, AccessControlType.Allow);
            }

            AddAclRuleOnDirectory(Installation.GetCacheDirectory(), everyone, rights, AccessControlType.Allow);
            AddAclRuleOnDirectory(Installation.GetLogDirectory(), everyone, rights, AccessControlType.Allow);
        }

        private static void AddAclRuleOnFile(string file, IdentityReference identity, FileSystemRights rights, AccessControlType type)
        {
            var acl = File.GetAccessControl(file);
            acl.PurgeAccessRules(identity);
            acl.AddAccessRule(new FileSystemAccessRule(identity, rights, type));
            File.SetAccessControl(file, acl);
        }

        private static void AddAclRuleOnDirectory(string directory, IdentityReference identity, FileSystemRights rights, AccessControlType type)
        {
            var acl = Directory.GetAccessControl(directory);
            acl.PurgeAccessRules(identity);
            acl.AddAccessRule(new FileSystemAccessRule(identity, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, type));
            Directory.SetAccessControl(directory, acl);
        }
    }
}
