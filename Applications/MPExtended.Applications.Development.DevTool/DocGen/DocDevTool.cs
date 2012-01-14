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
using System.IO;
using System.Reflection;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool.DocGen
{
    internal class DocDevTool : IDevTool
    {
        public TextWriter OutputStream { get; set; }
        public TextReader InputStream { get; set; }
        public string Name { get { return "DocGen"; } }

        public void Run()
        {
            string rootpath = Installation.GetSourceRootDirectory();
            OutputStream.Write("Enter output directory: ");
            string outputDir = InputStream.ReadLine().Trim();

            var generators = new List<Tuple<string, Type, string>>() {
                new Tuple<string, Type, string>("MediaAccessService", typeof(MASGenerator), "api-doc-mas.txt"),
                new Tuple<string, Type, string>("TVAccessService", typeof(TASGenerator), "api-doc-tas.txt"),
                new Tuple<string, Type, string>("StreamingService", typeof(WSSGenerator), "api-doc-wss.txt"),
            };

            // generate docs
            foreach (var generator in generators)
            {
                string projectName = "MPExtended.Services." + generator.Item1 + ".Interfaces";
                Assembly asm = Assembly.LoadFrom(Path.Combine(rootpath, "Services", projectName, "bin", "Debug", projectName + ".dll"));
                Generator gen = (Generator)Activator.CreateInstance(generator.Item2, asm);
                gen.Output = new StreamWriter(Path.Combine(outputDir, generator.Item3));
                gen.UserStream = OutputStream;
                gen.Generate();
            }
        }
    }
}
