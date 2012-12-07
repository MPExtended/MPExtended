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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MPExtended.Libraries.Service.Composition
{
    public static class AssemblyLoader
    {
        private static bool isInstalled = false;
        private static List<string> searchDirectories = new List<string>();
        private static Dictionary<string, List<Tuple<AssemblyName, string>>> knownAssemblies = new Dictionary<string, List<Tuple<AssemblyName, string>>>();

        public static void Install()
        {
            if (isInstalled)
                return;

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(ResolveAssembly);

            isInstalled = true;
        }

        public static void AddSearchDirectory(string directory)
        {
            searchDirectories.Add(directory);
        }

        public static void AddDependencyDirectory(Assembly referencingAssembly, string directory)
        {
            foreach (var dependency in referencingAssembly.GetReferencedAssemblies())
            {
                if (dependency.Name == "System" || dependency.Name.StartsWith("System.") || dependency.Name == "mscorlib")
                    continue;

                AddAssemblyLocation(dependency, directory);
            }
        }

        public static void AddAssemblyLocation(AssemblyName assembly, string directory)
        {
            if (!knownAssemblies.ContainsKey(assembly.Name))
                knownAssemblies[assembly.Name] = new List<Tuple<AssemblyName, string>>();
            knownAssemblies[assembly.Name].Add(Tuple.Create(assembly, directory));
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var requestedName = new AssemblyName(args.Name);

            if (knownAssemblies.ContainsKey(requestedName.Name))
            {
                foreach (var item in knownAssemblies[requestedName.Name])
                {
                    if (requestedName.Name == item.Item1.Name &&
                            (requestedName.Version == null || requestedName.Version == item.Item1.Version) &&
                            (requestedName.CultureInfo == null || requestedName.CultureInfo == item.Item1.CultureInfo) &&
                            (requestedName.GetPublicKey() == null || requestedName.GetPublicKey() == item.Item1.GetPublicKey()))
                    {
                        var path = Path.Combine(item.Item2, requestedName.Name + ".dll");
                        if (File.Exists(path))
                            return Assembly.LoadFrom(path);
                    }
                }
            }

            foreach (var directory in searchDirectories)
            {
                var path = Path.Combine(directory, requestedName.Name + ".dll");
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
            }

            return null;
        }
    }
}
