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
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool.Tools
{
    internal class MyGengoImporter : IDevTool, IQuestioningDevTool
    {
        public string Name { get { return "MyGengo resx importer"; } }
        public TextWriter OutputStream { get; set; }
        public Dictionary<string, string> Answers { get; set; }

        public IEnumerable<Question> Questions
        {
            get
            {
                return new List<Question>() {
                    new Question("sourceDir", "Enter directory to read source tree from: ")
                };
            }
        }

        public void Run()
        {
            string sourceDir = Answers["sourceDir"];

            // load all files
            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string language = Path.GetFileName(directory);
                if(language == "en")
                {
                    OutputStream.WriteLine("Skipping master English files");
                    continue;
                }
                
                foreach (var file in Directory.GetFiles(directory, "*.resx"))
                {
                    OutputStream.WriteLine("Processing {0} (language {1})", file, language);

                    // remove invalid items
                    XElement xml = XElement.Load(file);
                    xml.Elements("data").Where(data => String.IsNullOrEmpty(data.Element("value").Value)).Remove();
                    xml.Elements("data").Where(data => String.IsNullOrEmpty(data.Attribute("name").Value)).Remove();
                    if (xml.Elements("data").Count() == 0)
                    {
                        OutputStream.WriteLine("No translated strings found, skipping this file...");
                        continue;
                    }

                    // write to output file
                    string baseName = Path.GetFileNameWithoutExtension(file) + "." + language + ".resx";
                    string outputFile = Path.Combine(Installation.GetSourceRootDirectory(), GetSourceRootDirectory(Path.GetFileName(file)), baseName);
                    xml.Save(outputFile);
                    OutputStream.WriteLine("Saved output to {0}", outputFile);
                }
            }
        }

        public string GetSourceRootDirectory(string resxName)
        {
            switch (resxName)
            {
                case "UIStrings.resx":
                case "FormStrings.resx":
                    return Path.Combine("Applications", "MPExtended.Applications.WebMediaPortal", "Strings");
                default:
                    OutputStream.WriteLine("Failed to determine location of translation file {0}", resxName);
                    return null;
            }
        }
    }
}
