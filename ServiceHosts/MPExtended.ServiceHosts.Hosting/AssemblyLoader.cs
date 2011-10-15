#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.IO;
using System.Reflection;
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.Hosting
{
    public static class AssemblyLoader
    {
        private static Dictionary<string, string> unusualNames = new Dictionary<string, string>()
        {
            { "MySql.Data.resources", "MySql.Data" }
        };

        private static List<string> directories = null;

        public static Assembly LoadAssembly(object sender, ResolveEventArgs args)
        {
            if (directories == null)
            {
                directories = ServiceList.GetAvailableServices().Select(x => Path.GetDirectoryName(x.AssemblyPath)).ToList();
            }

            AssemblyName name = new AssemblyName(args.Name);
            string asmname = name.Name;
            if (unusualNames.ContainsKey(asmname))
            {
                asmname = unusualNames[asmname];
            }

            foreach (string dir in directories)
            {
                if (File.Exists(Path.Combine(dir, asmname + ".dll")))
                {
                    return Assembly.LoadFrom(Path.Combine(dir, asmname + ".dll"));
                }
            }

            Log.Warn("Failed to load assembly {0}", asmname);
            return null;
        }
    }
}
