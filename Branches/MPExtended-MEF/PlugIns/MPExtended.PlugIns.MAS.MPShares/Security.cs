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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;

namespace MPExtended.PlugIns.MAS.MPShares
{
    internal class Security
    {
        public static bool IsSubdir(string root, string testDir)
        {
            DirectoryInfo shareDir = new DirectoryInfo(root);
            DirectoryInfo currentDir = new DirectoryInfo(testDir);

            while (currentDir != null)
            {
                if (currentDir.FullName == shareDir.FullName)
                    return true;

                currentDir = currentDir.Parent;
            }

            return false;
        }

        public static bool IsAllowedPath(ILogger log, string path, IEnumerable<WebDriveBasic> shares)
        {
            try
            {
                // non-existent files don't yield results anyway, we might need to have them existent for the checks here later
                if (!File.Exists(path) && !Directory.Exists(path))
                    return false;

                // do not allow non-rooted parts for security
                if (!Path.IsPathRooted(path))
                    return false;

                // only check on directory
                if (File.Exists(path))
                    path = Path.GetDirectoryName(path);

                foreach (WebDriveBasic share in shares)
                {
                    if (!Directory.Exists(share.Path))
                        continue;

                    if (IsSubdir(share.Path, path))
                        return true;
                }
            }
            catch (Exception e)
            {
                log.Error(String.Format("Exception during IsAllowedPath with path = {0}", path), e);
            }
            return false;
        }
    }
}
