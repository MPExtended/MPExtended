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
using Microsoft.Win32;

namespace MPExtended.Libraries.General
{
    public class Service
    {
        public MPExtendedService ServiceName { get; set; }
        public string ImplementationName { get; set; }
        public string Assembly { get; set; }
        public string ZeroconfServiceType { get; set; }

        public virtual bool HostAsWCF
        {
            get
            {
                return true;
            }
        }

        public virtual bool IsInstalled
        {
            get
            {
                return Installation.GetFileLayoutType() == FileLayoutType.Source ||
                    RegistryReader.ReadKey(RegistryHive.LocalMachine, @"Software\MPExtended", ServiceName.ToString() + "Installed") != null;
            }
        }

        public virtual int Port
        {
            get
            {
                return Configuration.Services.Port;
            }
        }

        public virtual string AssemblyPath
        {
            get
            {
                return Installation.GetFileLayoutType() == FileLayoutType.Source ? 
                    Path.Combine(Installation.GetSourceRootDirectory(), "Services", Assembly, "bin", "Debug", Assembly + ".dll") :
                    Path.Combine(Installation.GetInstallDirectory(MPExtendedProduct.Service), Assembly + ".dll");
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

        public virtual void GetUsernameAndPassword(User user, out string username, out string password)
        {
            username = user.Username;
            password = user.GetPassword();
        }
    }
}
