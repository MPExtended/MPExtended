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

namespace MPExtended.Applications.Development.DevTool.WixFS
{
    internal class WebMPGenerator : WixFSGenerator, IQuestioningDevTool, IDevTool
    {
        public string Name { get { return "Wix FS (WebMP WWW) Generator"; } }
        public TextWriter OutputStream { get; set; }
        public Dictionary<string, string> Answers { get; set; }

        public IEnumerable<Question> Questions
        {
            get
            {
                return new List<Question>() {
                    new Question("outputDirectory", "Enter output directory: ")
                };
            }
        }

        public void Run()
        {
            forbiddenFiles = new string[] { "packages.config", "Web.Debug.config", "Web.Release.config" };
            forbiddenExtensions = new string[] { ".cs", ".csproj", ".user" };
            forbiddenDirectories = new string[] { "obj", "bin", "Skins", "Plugins", "Strings" };

            RunFromInput(@"Applications\MPExtended.Applications.WebMediaPortal", Answers["outputDirectory"] + "WWW.wxs", "WWW", "Dir_WWW", false);
            RunFromInput(@"Applications\MPExtended.Applications.WebMediaPortal\bin", Answers["outputDirectory"] + "Translations.wxs", "Translations", "BinDirectory", true);

            OutputStream.WriteLine("Done!");
        }
    }
}
