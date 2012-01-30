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
using System.IO;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service;
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

        public static bool IsAllowedPath(ILogger log, string path, IEnumerable<Share> shares)
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

                foreach (Share share in shares)
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

        public static bool IsInShare(string path, Share share)
        {
            try
            {
                // non-existent files don't yield results anyway, we might need to have them existent for the checks here later
                if (!File.Exists(path) && !Directory.Exists(path))
                    return false;

                // we should only get rooted paths, but check anyway
                if (!Path.IsPathRooted(path))
                    return false;

                // only check on parent directory path
                if (File.Exists(path))
                    path = Path.GetDirectoryName(path);

                // if share doesn't exist don't allow access
                if (!Directory.Exists(share.Path))
                    return false;

                // check if it's a subdir and log fails
                bool retval = IsSubdir(share.Path, path);
                if (!retval)
                {
                    Log.Warn("Tried to access file {0} outside share {1}", path, share.Path);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Exception druing IsInShare with path = {0}", path), ex);
            }
            return false;
        }
    }
}
