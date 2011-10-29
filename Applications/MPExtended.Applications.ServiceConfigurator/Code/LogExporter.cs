#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class LogExporter
    {
        public static void Export(string savePath)
        {
            // create zipfile
            using (var zipFile = ZipPackage.Open(savePath, FileMode.Create))
            {
                // copy log files
                DirectoryInfo logDir = new DirectoryInfo(Installation.GetLogDirectory());
                foreach (FileInfo file in logDir.GetFiles("*.log"))
                {
                    var logPart = zipFile.CreatePart(new Uri("/" + file.Name, UriKind.Relative), "", CompressionOption.Maximum);
                    File.OpenRead(file.FullName).CopyTo(logPart.GetStream());
                }

                // copy config files
                foreach(string name in new string[] { "MediaAccess.xml", "Streaming.xml" }) {
                    var configPart = zipFile.CreatePart(new Uri("/" + name, UriKind.Relative), "", CompressionOption.Maximum);
                    File.OpenRead(Path.Combine(Installation.GetConfigurationDirectory(), name)).CopyTo(configPart.GetStream());
                }

                // strip username & password from file
                var servicePart = zipFile.CreatePart(new Uri("/Services.xml", UriKind.Relative), "", CompressionOption.Maximum);
                XElement services = XElement.Load(Path.Combine(Installation.GetConfigurationDirectory(), "Services.xml"));
                services.Element("users").Elements("user").Remove();
                services.Element("users").Value = "Removed by ServiceConfigurator export";
                services.Save(servicePart.GetStream());
            }
        }
    }
}