#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.Hosting
{
    internal class Service
    {
        public MPExtendedService ServiceName { get; set; }
        public string ImplementationName { get; set; }
        public string Assembly { get; set; }
        public string ZeroconfServiceType { get; set; }

        public bool IsInstalled
        {
            get
            {
                return Installation.CheckInstalled(ServiceName);
            }
        }

        public string AssemblyPath
        {
            get
            {
#if DEBUG
                string bindir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string solutionRoot = Path.GetFullPath(Path.Combine(bindir, "..", "..", "..", ".."));
                string fullpath = Path.GetFullPath(Path.Combine(solutionRoot, "Services", Assembly, "bin", "Debug", Assembly + ".dll"));
                return fullpath;
#else
                string ourDirectory = AppDomain.CurrentDomain.BaseDirectory;
                return Path.GetFullPath(Path.Combine(ourDirectory, Assembly + ".dll"));
#endif
            }
        }

        public string FullTypeName
        {
            get
            {
                return Assembly + "." + ImplementationName;
            }
        }

        public Service(MPExtendedService srv, string assembly, string implementationName, string serviceType)
        {
            this.ServiceName = srv;
            this.Assembly = assembly;
            this.ImplementationName = implementationName;
            this.ZeroconfServiceType = serviceType;
        }
    }
}
