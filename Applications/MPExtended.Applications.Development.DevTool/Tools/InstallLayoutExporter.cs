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
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.Development.DevTool.Tools
{
    class InstallLayoutExporter : IQuestioningDevTool, IDevTool
    {
        private const string WIX_NS = "http://schemas.microsoft.com/wix/2006/wi";

        public string Name { get { return "Installed filelayout exporter"; } }
        public TextWriter OutputStream { get; set; }
        public Dictionary<string, string> Answers { get; set; }

        public IEnumerable<Question> Questions
        {
            get
            {
                return new List<Question>() {
                    new Question("outputDirectory", "Enter output directory: "),
                    new Question("onlyMPExtended", "Only MPExtended code (ignore bundled libraries)? [y/n] ")
                };
            }
        }

        public void Run()
        {
            string outputDirectory = Answers["outputDirectory"];
            bool onlyMPExtended = Answers["onlyMPExtended"] == "y";

            string filesystemWxs = Path.Combine(Installation.GetSourceRootDirectory(), "Installers", "MPExtended.Installers.Service", "Filesystem.wxs");
            XElement root = XElement.Load(filesystemWxs);
            var dir = root.Descendants(XName.Get("Directory", WIX_NS)).First(d => d.Attribute("Id") != null && d.Attribute("Id").Value == "InstallDirectory");
            GenerateTreeFromNode(dir, outputDirectory, onlyMPExtended);
        }

        private void GenerateTreeFromNode(XElement node, string directory, bool onlyMPExtended)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (var component in node.Elements(XName.Get("Component", WIX_NS)))
            {
                foreach (var file in component.Elements(XName.Get("File", WIX_NS)))
                {
                    string source = ToFullPath(file.Attribute("Source").Value);
                    if (!onlyMPExtended || Path.GetFileName(source).Contains("MPExtended"))
                    {
                        OutputStream.WriteLine("Adding {0}", Path.GetFileName(source));
                        File.Copy(source, Path.Combine(directory, Path.GetFileName(source)));
                    }
                }
            }

            // child directories
            foreach (var dir in node.Elements(XName.Get("Directory", WIX_NS)))
            {
                string childDir = Path.Combine(directory, dir.Attribute("Name").Value);
                GenerateTreeFromNode(dir, childDir, onlyMPExtended);
            }
        }

        private string ToFullPath(string wixpath)
        {
            // standard replacements
            wixpath = wixpath
                .Replace("$(var.SolutionDir)", Installation.GetSourceRootDirectory());

            // replace project targetdir
            var result = Regex.Matches(wixpath, @"\$\(var\.(MPExtended\.([a-z]+)\.[a-z.]+)\.TargetDir\)", RegexOptions.IgnoreCase);
            foreach (Match match in result)
            {
                string rep = Path.Combine(Installation.GetSourceRootDirectory(), match.Groups[2].Value, match.Groups[1].Value, "bin", Installation.GetSourceBuildDirectoryName());
                wixpath = wixpath.Replace(match.Value, rep);
            }

            // return, but error out if there are still wix substitutions
            if (wixpath.Contains("$("))
            {
                throw new ArgumentException();
            }
            return wixpath;
        }
    }
}
