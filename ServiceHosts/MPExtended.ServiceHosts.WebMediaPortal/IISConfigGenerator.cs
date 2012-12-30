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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.Service;

namespace MPExtended.ServiceHosts.WebMediaPortal
{
    internal class IISConfigGenerator
    {
        public string TemplatePath { get; set; }
        public string PhysicalSitePath { get; set; }
        public string TemporaryPath { get; set; }

        public bool GenerateConfigFile(string outputFile)
        {
            try
            {
                // create config
                XElement file = XElement.Load(TemplatePath);
                XElement site = file.Element("system.applicationHost").Element("sites").Elements("site").First(x => x.Attribute("name").Value == "WebMediaPortal");

                site.Element("application").Add(
                    new XElement("virtualDirectory",
                        new XAttribute("path", "/"),
                        new XAttribute("physicalPath", PhysicalSitePath)
                    )
                );

                site.Element("bindings").Add(
                    new XElement("binding",
                        new XAttribute("protocol", "http"),
                        new XAttribute("bindingInformation", String.Format("*:{0}:", Configuration.WebMediaPortalHosting.Port))
                    )
                );
                Log.Debug("Wrote binding for http on port {0}", Configuration.WebMediaPortalHosting.Port);

                if (Configuration.WebMediaPortalHosting.EnableTLS)
                {
                    site.Element("bindings").Add(
                        new XElement("binding",
                            new XAttribute("protocol", "https"),
                            new XAttribute("bindingInformation", String.Format("*:{0}:", Configuration.WebMediaPortalHosting.PortTLS))
                        )
                    );
                    Log.Debug("Wrote binding for https on port {0}", Configuration.WebMediaPortalHosting.PortTLS);
                }

                // some cache directories
                CreateDirectoryIfNonExistent(TemporaryPath);

                var compiledTemplates = Path.Combine(TemporaryPath, "ASP Compiled Templates");
                CreateDirectoryIfNonExistent(compiledTemplates);
                file.Element("system.webServer")
                    .Element("asp").Add(
                        new XElement("cache",
                            new XAttribute("diskTemplateCacheDirectory", compiledTemplates)
                        )
                    );

                var temporaryCacheDir = Path.Combine(TemporaryPath, "Temporary Compressed Files");
                CreateDirectoryIfNonExistent(temporaryCacheDir);
                file.Element("system.webServer")
                    .Element("httpCompression")
                    .Add(new XAttribute("directory", temporaryCacheDir));

                file.Save(outputFile);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to generate IIS config file", ex);
                return false;
            }
        }

        private void CreateDirectoryIfNonExistent(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
