#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool.WixFS
{
    internal class CustomGenerator : WixFSGenerator, IDevTool, IQuestioningDevTool
    {
        public string Name { get { return "Wix FS Generator"; } }
        public TextWriter OutputStream { get; set; }
        public Dictionary<string, string> Answers { get; set; }

        public IEnumerable<Question> Questions
        {
            get
            {
                return new List<Question>() {
                    new Question("inputDirectory", "Enter directory to include: " + Installation.GetSourceRootDirectory() + @"\"),
                    new Question("outputFile", "Enter output file: "),
                    new Question("prefix", "Enter prefix of the directory and component: "),
                    new Question("parentName", "Enter id of parent directory: "),
                    new Question("noImmediate", "Skip immediate files (only include subdirectories)? "),
                };
            }
        }

        public void Run()
        {
            forbiddenFiles = new string[] { };
            forbiddenExtensions = new string[] { };
            forbiddenDirectories = new string[] { };

            RunFromInput(Answers["inputDirectory"], Answers["outputFile"], Answers["prefix"], Answers["parentName"], Answers["noImmediate"] == "y");

            OutputStream.WriteLine("Done!");
        }
    }
}
