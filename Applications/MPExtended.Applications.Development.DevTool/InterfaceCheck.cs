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
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool
{
    internal class InterfaceCheck : IDevTool
    {
        public TextWriter OutputStream { get; set; }
        public TextReader InputStream { get; set; }
        public string Name { get { return "InterfaceCheck"; } }

        public void Run()
        {
            string rootpath = Installation.GetSourceRootDirectory();
            CheckService(rootpath, "MPExtended.Services.MediaAccessService", "IMediaAccessService", "MediaAccessService");
            CheckService(rootpath, "MPExtended.Services.TVAccessService", "ITVAccessService", "TVAccessService");
            CheckService(rootpath, "MPExtended.Services.StreamingService", new string[] { "IStreamingService", "IWebStreamingService" }, "StreamingService");
            CheckService(rootpath, "MPExtended.Services.UserSessionService", "IUserSessionService", "UserSessionService");
            OutputStream.WriteLine("Done");
        }

        private void CheckService(string rootpath, string service, string interfacename, string classname)
        {
            CheckService(rootpath, service, new string[] { interfacename }, classname);
        }

        private void CheckService(string rootpath, string service, string[] interfacenames, string classname)
        {
            string interfaceAssemblyName = service + ".Interfaces";
            Assembly iface = Assembly.LoadFrom(Path.Combine(rootpath, "Services", interfaceAssemblyName, "bin", "Debug", interfaceAssemblyName + ".dll"));
            Assembly impl = Assembly.LoadFrom(Path.Combine(rootpath, "Services", service, "bin", "Debug", service + ".dll"));

            Type[] interfaceTypes = interfacenames.Select(x => iface.GetType(interfaceAssemblyName + "." + x)).ToArray();
            Type implementationType = impl.GetType(service + "." + classname);

            MethodInfo[] implMethods = implementationType.GetMethods();
            MethodInfo[] intfMethods = interfaceTypes.SelectMany(x => x.GetMethods()).ToArray();

            foreach (MethodInfo currentMethod in implMethods)
            {
                // skip standard methods
                if (currentMethod.DeclaringType.Name != implementationType.Name || currentMethod.Name == "Dispose")
                {
                    continue;
                }

                if (intfMethods.Where(x => x.Name == currentMethod.Name).Count() == 0)
                {
                    OutputStream.WriteLine("{0}: Unknown method {1} in implementation!", service, currentMethod.Name);
                    continue;
                }

                MethodInfo intf = intfMethods.Where(x => x.Name == currentMethod.Name).First();
                ParameterInfo[] intfParameters = intf.GetParameters();
                ParameterInfo[] implParameters = currentMethod.GetParameters();

                if (intfParameters.Length != implParameters.Length)
                {
                    OutputStream.WriteLine("{0}: Method {1} exists in implementation with different signature then in interface", service, currentMethod.Name);
                    continue;
                }

                for (int i = 0; i < intfParameters.Length; i++)
                {
                    if (intfParameters[i].Name != implParameters[i].Name)
                    {
                        OutputStream.WriteLine("{0}({1}): Parameter {2} has different names ({3} vs {4})", service, currentMethod.Name, i, intfParameters[i].Name, implParameters[i].Name);
                    }
                }
            }
        }
    }
}
