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
using System.Reflection;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;

namespace MPExtended.Tests.Libraries.Service
{
    public class TestEnvironment
    {
        public static InstallationProperties CreateProperties()
        {
            var prop = new InstallationProperties();
            prop.Product = MPExtendedProduct.Service;
            prop.FileLayout = FileLayoutType.Source;
            prop.SourceRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "")), "../../../.."));
            prop.CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Cache");
            prop.LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MPExtended", "Logs");
            prop.ConfigurationDirectory = Path.Combine(prop.SourceRoot, "Config");
            prop.DefaultConfigurationDirectory = prop.ConfigurationDirectory;
            prop.ConfigurationBackupDirectory = Path.Combine(prop.ConfigurationDirectory, "ConfigBackup");
            prop.SourceBuildDirectory = "Debug";
            return prop;
        }
    }
}
