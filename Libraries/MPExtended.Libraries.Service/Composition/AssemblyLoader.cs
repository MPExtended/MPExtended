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
using System.Reflection;
using System.Text;

namespace MPExtended.Libraries.Service.Composition
{
    internal static class AssemblyNameExtensionMethods
    {
        public static bool IsCompatibleWith(this AssemblyName ourName, AssemblyName requestedName)
        {
            return requestedName.Name == ourName.Name &&
                (requestedName.Version == null || requestedName.Version == ourName.Version) &&
                (requestedName.CultureInfo == null || requestedName.CultureInfo == ourName.CultureInfo) &&
                (requestedName.GetPublicKey() == null || requestedName.GetPublicKey() == ourName.GetPublicKey());
        }
    }

    public static class AssemblyLoader
    {
        private static bool isInstalled = false;
        private static List<string> searchDirectories = new List<string>();
        private static Dictionary<string, List<Tuple<AssemblyName, string>>> knownAssemblies = new Dictionary<string, List<Tuple<AssemblyName, string>>>();
        private static Dictionary<AssemblyName, Assembly> loadedAssemblies = new Dictionary<AssemblyName, Assembly>();

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

        public static Assembly LoadAssembly(string assemblyName, string suggestionPath)
        {
            var requestedName = new AssemblyName(assemblyName);

            foreach (var item in loadedAssemblies)
            {
                if (item.Key.IsCompatibleWith(requestedName))
                    return item.Value;
            }

            return LoadAndSaveAssembly(requestedName, suggestionPath);
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var requestedName = new AssemblyName(args.Name);

            if (knownAssemblies.ContainsKey(requestedName.Name))
            {
                foreach (var item in knownAssemblies[requestedName.Name])
                {
                    if (item.Item1.IsCompatibleWith(requestedName))
                    {
                        var path = Path.Combine(item.Item2, requestedName.Name + ".dll");
                        if (File.Exists(path))
                            return LoadAndSaveAssembly(requestedName, path);
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

        private static Assembly LoadAndSaveAssembly(AssemblyName name, string path)
        {
            var assembly = Assembly.LoadFrom(path);
            var loadedName = assembly.GetName();
            loadedAssemblies[loadedName] = assembly;
            if (loadedName.IsCompatibleWith(name))
                return assembly;

            Log.Error("Failed to load {0} from {1}, as it contains assembly {2}", name, path, loadedName);
            return null;
        }
    }
}
