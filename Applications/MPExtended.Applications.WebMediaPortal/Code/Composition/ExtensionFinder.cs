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
using System.Web;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code.Composition
{
    internal abstract class ExtensionFinder
    {
        private string rootDirectory;

        public ExtensionFinder()
        {
            rootDirectory = WebMediaPortalApplication.GetInstallationDirectory();
        }

        protected abstract string GetExtensionDirectoryName();

        public string GetParentDirectory()
        {
            return Path.Combine(rootDirectory, GetExtensionDirectoryName());
        }

        public virtual IEnumerable<string> GetDirectories()
        {
            if (!Directory.Exists(GetParentDirectory()))
                return new List<string>();

            return Directory.GetDirectories(GetParentDirectory());
        }
    }
}