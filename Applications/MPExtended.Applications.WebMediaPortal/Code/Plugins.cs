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
using System.Linq;
using System.IO;
using System.Web;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    internal static class Plugins
    {
        public static string[] ListPluginDirectories()
        {
            string webmpDirectory =
                Installation.GetFileLayoutType() == FileLayoutType.Source ?
                    Path.Combine(Installation.GetSourceRootDirectory(), "Applications", "MPExtended.Applications.WebMediaPortal") :
                    Path.Combine(Installation.GetInstallDirectory(MPExtendedProduct.WebMediaPortal), "www");
            string pluginDirectory = Path.Combine(webmpDirectory, "Plugins");
            if (!Directory.Exists(pluginDirectory))
                return new string[] { };

            return Directory.GetDirectories(pluginDirectory);
        }

        public static string[] ListPlugins()
        {
            return ListPluginDirectories()
                .Select(x => Path.GetDirectoryName(x))
                .ToArray();
        }
    }
}