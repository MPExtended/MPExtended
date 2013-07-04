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
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MPExtended.Libraries.Service;

namespace MPExtended.ServiceHosts.WebMediaPortal
{
    /* I'm sorry. I'm so sorry.
     * 
     * This hack is "needed" for two reasons:
     * 1. On Windows 8 and up, the SYSTEM user doesn't have write permissions to the default
     *    "Temporary ASP.NET Files" directory, so we need to either:
     *    (a) Run as a user that does have write permissions. This is hard to impossible because
     *        IIS Express doesn't support changing users, and the other option is running the service
     *        as the different user, but that'd require Wix changes...
     *    (b) Change the permissions on the directory. This feels quite icky and we should try to
     *        avoid changing permissions on system directories.
     *    (c) Use another directory. This directory can be set with the tempDirectory attribute on the
     *        <compilation> tag in the <system.web> section of Web.config.
     * 2. We cannot hardcode a path to the cache directory in the Web.config shipped with WebMediaPortal,
     *    since we don't know what the cache directory is: it's either in C:\ProgramData (Windows Vista+), 
     *    or somewhere deep into the common user profile (Windows XP).
     * 3. We can't put a placeholder there either, since the plain web.config is used in the IIS edition
     *    and during development as well, and it should work without any modifications then. We don't hit
     *    the permission issue in these cases, since IIS and Visual Studio use respectively solution (b)
     *    and (a). This is part of what aspnet_regiis.exe does.
     *    
     * Again, I'm sorry for the ugly hack we ended up with.
     */
    internal class WebConfigFixer
    {
        private string _configPath;

        public WebConfigFixer(string configPath)
        {
            _configPath = configPath;
        }

        public void WriteFixedVersionIfNeeded()
        {
            XElement file = XElement.Load(_configPath);
            var compilationNode = file.Element("system.web").Element("compilation");

            if (compilationNode.Attribute("tempDirectory") != null)
                return;

            var tempDirectory = Path.Combine(Installation.GetCacheDirectory(), "CompilationTemp");
            compilationNode.Add(new XAttribute("tempDirectory", tempDirectory));
            file.Save(_configPath);
            Log.Info("Set temporary ASP.NET directory to {0} in Web.config file", tempDirectory);
        }
    }
}
