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
    internal class DocDevTool : IDevTool, IQuestioningDevTool
    {
        public string Name { get { return "Documentation generator"; } }
        public TextWriter OutputStream { get; set; }
        public Dictionary<string, string> Answers { get; set; }

        public IEnumerable<Question> Questions
        {
            get
            {
                return new List<Question>() {
                    new Question("outputDirectory", "Enter output directory: "),
                };
            }
        }

        public void Run()
        {
            string rootpath = Installation.GetSourceRootDirectory();
            string outputDir = Answers["outputDirectory"];

            var generators = new List<Tuple<string, Type, string>>() {
                new Tuple<string, Type, string>("MediaAccessService", typeof(MASGenerator), "api-doc-mas.txt"),
                new Tuple<string, Type, string>("TVAccessService", typeof(TASGenerator), "api-doc-tas.txt"),
                new Tuple<string, Type, string>("StreamingService", typeof(WSSGenerator), "api-doc-wss.txt"),
            };

            // generate docs
            foreach (var generator in generators)
            {
                string projectName = "MPExtended.Services." + generator.Item1 + ".Interfaces";
                Assembly asm = Assembly.LoadFrom(Path.Combine(rootpath, "Services", projectName, "bin", Installation.GetSourceBuildDirectoryName(), projectName + ".dll"));
                Generator gen = (Generator)Activator.CreateInstance(generator.Item2, asm);
                gen.Output = new StreamWriter(Path.Combine(outputDir, generator.Item3));
                gen.UserStream = OutputStream;
                gen.Generate();
            }
        }
    }
}
