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
using System.Security.Principal;
using System.Text;
using System.IO;
using Microsoft.Win32;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Network
{
    public class MappedDriveConverter
    {
        private Dictionary<string, Dictionary<string, string>> driveCache = new Dictionary<string,Dictionary<string,string>>();

        public string GetUncPathForDrive(string domain, string user, string drive)
        {
            var userIdentifier = String.Format(@"{0}\{1}", domain, user);
            if (!driveCache.ContainsKey(userIdentifier))
            {
                var account = new NTAccount(domain, user);
                if (account == null)
                    return null;
                var sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
                if (sid == null)
                    return null;

                var drives = RegistryReader.GetSubKeys(RegistryHive.Users, RegistryView.Default, String.Format(@"{0}\Network", sid.Value));
                if (drives == null)
                    return null;

                driveCache[userIdentifier] = drives.Select(driveLetter => {
                    string uncPath = RegistryReader.ReadKey(RegistryHive.Users, RegistryView.Default, String.Format(@"{0}\Network\{1}", sid.Value, driveLetter), "RemotePath").ToString();
                    return new KeyValuePair<string, string>(driveLetter, uncPath);
                }).Where(x => x.Value != null).ToDictionary();
            }

            if (driveCache[userIdentifier].ContainsKey(drive))
                return driveCache[userIdentifier][drive];

            return null;
        }

        public string ConvertPathToUnc(string domain, string user, string path)
        {
            var root = Path.GetPathRoot(path);
            if (!root.Contains(@":\"))
                return path;

            var driveLetter = root.Substring(0, root.IndexOf(@":\"));
            var uncPath = GetUncPathForDrive(domain, user, driveLetter);
            if (uncPath == null)
                return path;

            return path.Replace(root, uncPath + @"\");
        }
    }
}
