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

        public SafeDirectoryCatalog(string directory)
        {
            aggregateCatalog = new AggregateCatalog();

            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var asmCatalog = new AssemblyCatalog(file);

                    // Force MEF to load the assembly file now, so that we can catch any exceptions occuring because of broken
                    // plugins, and skip those plugins.
                    asmCatalog.Parts.ToArray();

                    aggregateCatalog.Catalogs.Add(asmCatalog);
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
    }
}
