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
using Microsoft.Xml.Serialization.GeneratedAssembly;

namespace MPExtended.Libraries.Service.Config
{
    internal class ProfilesConfigurationSerializer : ConfigurationSerializer<StreamingProfiles, StreamingProfilesSerializer>
    {
        public ProfilesConfigurationSerializer()
            : base(ConfigurationFile.StreamingProfiles, "StreamingProfiles.xml")
        {
        }

        protected override StreamingProfiles ReadFromDisk()
        {
            var currentInstance = base.ReadFromDisk();

            try
            {
                string defaultPath = Path.Combine(Installation.Properties.DefaultConfigurationDirectory, Filename);
                var newInstance = UnsafeParse(defaultPath);
                if (!currentInstance.Customized && currentInstance.ProfilesVersion < newInstance.ProfilesVersion)
                {
                    Log.Info("Replacing uncustomized current StreamingProfiles.xml version {0} with version {1}", currentInstance.ProfilesVersion, newInstance.ProfilesVersion);
                    File.Copy(defaultPath, Path.Combine(Installation.Properties.ConfigurationDirectory, Filename));
                    return newInstance;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to read default StreamingProfiles.xml, keep using current file.", ex);
            }

            return currentInstance;
        }
    }
}
