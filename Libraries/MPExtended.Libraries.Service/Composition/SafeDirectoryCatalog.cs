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
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MPExtended.Libraries.Service.Composition
{
    internal class SafeDirectoryCatalog : ComposablePartCatalog
    {
        private readonly AggregateCatalog aggregateCatalog;

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return aggregateCatalog.Parts; }
        }

        public SafeDirectoryCatalog(string directory, string searchPattern, bool recursive)
        {
            if (searchPattern == null)
                searchPattern = "*.dll";

            aggregateCatalog = new AggregateCatalog();

            // Interestingly enough, we need to load the interface libraries first, or we'll get an error about unimplemented types, probably
            // because the interface library might already have been loaded from another location (via the MPExtended.Libraries.Service ->
            // MPExtended.Libraries.Client -> Interface dependency), even though that shouldn't matter. Whatever, it's 1:25 am and it works
            // this way.
            var files = Directory.EnumerateFiles(directory, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                                 .OrderByDescending(x => x.EndsWith(".Interfaces.dll"))
                                 .ThenBy(x => x);
            foreach (var file in files)
            {
                try
                {
                    // Force MEF to load and scan the assembly file now, so that we can catch any exceptions occuring 
                    // because of broken plugins, and skip those plugins.
                    var assemblyName = Path.GetFileNameWithoutExtension(file);
                    var assembly = AssemblyLoader.LoadAssembly(assemblyName, file);
                    var asmCatalog = new AssemblyCatalog(assembly);
                    asmCatalog.Parts.ToArray();
                    aggregateCatalog.Catalogs.Add(asmCatalog);

                    AssemblyLoader.Install();
                    AssemblyLoader.AddDependencyDirectory(assembly, directory);
                }
                catch (BadImageFormatException)
                {
                    Log.Trace("SafeDirectoryCatalog: BadImageFormatException for assembly {0}", file);
                }
                catch (ReflectionTypeLoadException)
                {
                    Log.Trace("SafeDirectoryCatalog: ReflectionTypeLoadException for assembly {0}", file);
                }
                catch (FileNotFoundException)
                {
                    Log.Trace("SafeDirectoryCatalog: FileNotFoundException for assembly {0}", file);
                }
            }
        }

        public SafeDirectoryCatalog(string directory, string searchPattern)
            : this (directory, searchPattern, true)
        {
        }

        public SafeDirectoryCatalog(string directory)
            : this(directory, null, true)
        {
        }
    }
}
