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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.Service.Config;

namespace MPExtended.Libraries.Service.Util
{
    public class WifiRemote
    {
        public static bool IsInstalled
        {
            get
            {
                try
                {
                    if (Mediaportal.HasValidConfigFile())
                    {
                        var config = Mediaportal.ReadSectionFromConfigFile("plugins");
                        return config.ContainsKey("WifiRemote") && config["WifiRemote"] == "yes";
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static int Port
        {
            get
            {
                try
                {
                    if (!Mediaportal.IsSectionInConfigFile("WifiRemote"))
                        return 8017;

                    string port = Mediaportal.ReadSectionFromConfigFile("WifiRemote")["port"];
                    return Int32.Parse(port);
                }
                catch (Exception)
                {
                    return 8017;
                }
            }
        }

        public static string ZeroconfName
        {
            get
            {
                return "_mepo-remote._tcp";
            }
        }

        public static User GetAuthentication()
        {
            User user = new User();

            try
            {
                user.Username = DecryptConfig(Mediaportal.ReadSectionFromConfigFile("WifiRemote")["username"]);
                user.SetPasswordFromPlaintext(DecryptConfig(Mediaportal.ReadSectionFromConfigFile("WifiRemote")["password"]));
            }
            catch (Exception)
            {
                user.Username = String.Empty;
                user.SetPasswordFromPlaintext(String.Empty);
            }

            return user;
        }

        private static string DecryptConfig(string data)
        {
            // I don't really get why this is used at all, since it doesn't provide any security. The key is static and can be found in the MP sources, and
            // 3DES isn't very secure anymore. Also, hashing is a lot better to use here as we never need the plaintext variant. Well, we've to deal with 
            // it. More or less taken from mediaportal/Core/Util/EncryptDecrypt.cs in the MP sources, it's at least where the encoder resides, so we've to
            // be compatible with it.

            byte[] dataArray = Convert.FromBase64String(data);
            var tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = UTF8Encoding.BigEndianUnicode.GetBytes("MPcrypto");
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            var transform = tdes.CreateDecryptor();
            byte[] resultArray = transform.TransformFinalBlock(dataArray, 0, dataArray.Length);
            tdes.Clear();
            return UTF8Encoding.BigEndianUnicode.GetString(resultArray);
        }
    }
}
