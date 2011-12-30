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
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace MPExtended.Libraries.General
{
    public class Configuration
    {
        private static ServicesConfiguration serviceConfig = null;
        private static MediaAccessConfiguration mediaConfig = null;
        private static StreamingConfiguration streamConfig = null;

        public static ServicesConfiguration Services 
        {
            get 
            {
                if (serviceConfig == null)
                    serviceConfig = new ServicesConfiguration();

                return serviceConfig;
            }
        }

        public static MediaAccessConfiguration Media
        {
            get
            {
                if (mediaConfig == null)
                    mediaConfig = new MediaAccessConfiguration();

                return mediaConfig;
            }
        }

        public static StreamingConfiguration Streaming
        {
            get
            {
                if (streamConfig == null)
                    streamConfig = new StreamingConfiguration();

                return streamConfig;
            }
        }

        public static string GetPath(string filename)
        {
            string path = Path.Combine(Installation.GetConfigurationDirectory(), filename);

            if (!File.Exists(path))
            {
#if DEBUG
                // In debug mode files always exists as they're read from the source tree
                throw new FileNotFoundException("Couldn't find config - what did you do?!?!");
#else
                // copy from default location
                MPExtendedProduct product = filename.StartsWith("WebMediaPortal") ? MPExtendedProduct.WebMediaPortal : MPExtendedProduct.Service;
                string defaultPath = Path.Combine(Installation.GetInstallDirectory(product), "DefaultConfig", filename);
                File.Copy(defaultPath, path);

                // allow everyone to write to the config
                var acl = File.GetAccessControl(path);
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                FileSystemAccessRule rule = new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow);
                acl.AddAccessRule(rule);
                File.SetAccessControl(path, acl);
#endif
            }

            return path;
        }

        internal static string PerformFolderSubstitution(string input)
        {
            string cappdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            input = input.Replace("%ProgramData%", cappdata);
            return input;
        }
    }
}
