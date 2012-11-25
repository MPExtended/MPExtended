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
using System.Security.Cryptography;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Config
{
    internal static class TransformationCallbacks
    {
        private const string PASSWORD_KEY = "MPExtended Password Key"; // And here all our security goes out of the window: the key is on the internet.

        internal static void Install()
        {
            Transformations.RegisterCallback(Transformations.Transformation.FolderNames, FolderSubstitution);
            Transformations.RegisterCallback(Transformations.Transformation.Encrypt, Encrypt);
            Transformations.RegisterCallback(Transformations.Transformation.Decrypt, Decrypt);
        }

        internal static string FolderSubstitution(string input)
        {
            // program data
            input = input.Replace("%ProgramData%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

            // streaming directory
            string streamingDirectory = Installation.GetFileLayoutType() == FileLayoutType.Source ?
                Path.Combine(Installation.GetSourceRootDirectory(), "Libraries", "Streaming") :
                Path.Combine(Installation.GetInstallDirectory(), "Streaming");
            input = input.Replace("%mpextended-streaming%", streamingDirectory);

            // mp settings
            input = Regex.Replace(input, @"%mp-([^-]+)-([^-]+)%", delegate(Match match)
            {
                var section = Mediaportal.ReadSectionFromConfigFile(match.Groups[1].Value);
                if (!section.ContainsKey(match.Groups[2].Value))
                {
                    Log.Info("Replacing unknown Mediaportal path substitution %mp-{0}-{1}% with empty string", match.Groups[1].Value, match.Groups[2].Value);
                    return String.Empty;
                }
                else
                {
                    return section[match.Groups[2].Value];
                }
            });

            return input;
        }

        internal static string Encrypt(string input)
        {
            return Encryption.Encrypt(PASSWORD_KEY, input);
        }

        internal static string Decrypt(string input)
        {
            try
            {
                return Encryption.Decrypt(PASSWORD_KEY, input);
            }
            catch (CryptographicException ex)
            {
                Log.Warn("Failed to decrypt string from configuration file", ex);
                return String.Empty;
            }
        }
    }
}
