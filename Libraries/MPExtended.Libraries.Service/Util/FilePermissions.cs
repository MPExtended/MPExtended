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
using System.Security.Principal;
using System.Text;

namespace MPExtended.Libraries.Service.Util
{
    public static class FilePermissions
    {
        public static IdentityReference EveryoneIdentity
        {
            get
            {
                return new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            }
        }

        public static void SetAclForUserOnFile(string file, IdentityReference identity, FileSystemRights rights, AccessControlType type)
        {
            var acl = File.GetAccessControl(file);
            acl.PurgeAccessRules(identity);
            acl.AddAccessRule(new FileSystemAccessRule(identity, rights, type));
            File.SetAccessControl(file, acl);
        }

        public static void SetAclForUserOnDirectory(string directory, IdentityReference identity, FileSystemRights rights, AccessControlType type)
        {
            var acl = Directory.GetAccessControl(directory);
            acl.PurgeAccessRules(identity);
            acl.AddAccessRule(new FileSystemAccessRule(identity, rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, type));
            Directory.SetAccessControl(directory, acl);
        }
    }
}
