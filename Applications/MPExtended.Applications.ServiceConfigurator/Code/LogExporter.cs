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
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class LogExporter
    {
        private const string PasswordSubstitute = "Removed by ServiceConfigurator export";

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
                    File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite).CopyTo(logPart.GetStream());
                }

                // copy MediaAccess.xml
                var masPart = zipFile.CreatePart(new Uri("/MediaAccess.xml", UriKind.Relative), "", CompressionOption.Maximum);
                File.OpenRead(Path.Combine(Installation.GetConfigurationDirectory(), "MediaAccess.xml")).CopyTo(masPart.GetStream());

                // strip watch sharing username and password from Streaming.xml
                var streamingPart = zipFile.CreatePart(new Uri("/Streaming.xml", UriKind.Relative), "", CompressionOption.Maximum);
                XElement streaming = XElement.Load(Path.Combine(Installation.GetConfigurationDirectory(), "Streaming.xml"));
                streaming.Element("watchsharing").Element("trakt").Element("passwordHash").Value = PasswordSubstitute;
                streaming.Element("watchsharing").Element("follwit").Element("passwordHash").Value = PasswordSubstitute;
                streaming.Save(streamingPart.GetStream());

                // strip username & passwords from Services.xml file
                var servicePart = zipFile.CreatePart(new Uri("/Services.xml", UriKind.Relative), "", CompressionOption.Maximum);
                XElement services = XElement.Load(Path.Combine(Installation.GetConfigurationDirectory(), "Services.xml"));
                services.Element("users").Elements("user").Remove();
                services.Element("users").Value = PasswordSubstitute;
                services.Element("networkImpersonation").Element("password").Value = PasswordSubstitute;
                services.Save(servicePart.GetStream());
            }
        }

        public static void ExportWithFileChooser()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".zip";
            dlg.Filter = Strings.UI.LogAndConfigurationArchive + "|*.zip";
            if (dlg.ShowDialog() == true)
            {
                Export(dlg.FileName);
                MessageBox.Show(String.Format(Strings.UI.ExportedLogsAndConfig, dlg.FileName), "MPExtended", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}