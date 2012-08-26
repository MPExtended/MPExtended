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

namespace MPExtended.Tests.Libraries.Service.Config
{
    public class ConfigurationTestEnvironment : IDisposable
    {
        protected string tempDirectory;

        public ConfigurationTestEnvironment()
        {
            var rootDirectory = Path.Combine(Path.GetTempPath(), "mpextended-tests");
            if (!Directory.Exists(rootDirectory))
                Directory.CreateDirectory(rootDirectory);

            tempDirectory = Path.Combine(rootDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);

            var properties = TestEnvironment.CreateProperties();
            properties.ConfigurationDirectory = tempDirectory;
            properties.DefaultConfigurationDirectory = tempDirectory;
            properties.ConfigurationBackupDirectory = Path.Combine(tempDirectory, "Backup");
            Installation.Properties = properties;
        }

        protected void AddConfigurationFile(string name, string path)
        {
            string resourceName = "MPExtended.Tests.Libraries.Service.Config.Data." + path.Replace('/', '.');
            WriteResourceToFile(resourceName, Path.Combine(tempDirectory, name));
        }

        protected void AddDefaultConfigurationFile(string name, string path)
        {
            if (!Directory.Exists(Path.Combine(tempDirectory, "Default")))
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "Default"));
                Installation.Properties.DefaultConfigurationDirectory = Path.Combine(tempDirectory, "Default");
            }

            string resourceName = "MPExtended.Tests.Libraries.Service.Config.Data." + path.Replace('/', '.');
            WriteResourceToFile(resourceName, Path.Combine(tempDirectory, "Default", name));
        }

        private static void WriteResourceToFile(string resourceName, string path)
        {
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (Stream fileStream = File.OpenWrite(path))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

        public void Dispose()
        {
            Directory.Delete(tempDirectory, true);
            Configuration.Reset();
        }
    }
}
