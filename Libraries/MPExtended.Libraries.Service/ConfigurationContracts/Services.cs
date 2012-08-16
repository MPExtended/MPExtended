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
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.ConfigurationContracts
{
    public class User
    {
        private const string PASSWORD_KEY = "MPExtended Password Key"; // And here all our security goes out of the window: the key is on the internet.

        public User()
        {
        }

        public User(string username, string plainTextPassword)
        {
            Username = username;
            SetPasswordFromPlaintext(plainTextPassword);
        }

        public string Username { get; set; }
        public string EncryptedPassword { get; set; }

        public bool ValidatePassword(string password)
        {
            return GetPassword() == password;
        }

        public string GetPassword()
        {
            return Encryption.Decrypt(PASSWORD_KEY, EncryptedPassword);
        }

        public void SetPasswordFromPlaintext(string password)
        {
            EncryptedPassword = Encryption.Encrypt(PASSWORD_KEY, password);
        }
    }

    public class NetworkImpersonation
    {
        private const string PASSWORD_KEY = "MPExtended Impersonation Password"; // And here all our security goes out of the window: the key is on the internet.

        public string Domain { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public bool ReadInStreamingService { get; set; }

        public string GetPassword()
        {
            return EncryptedPassword == String.Empty ? String.Empty : Encryption.Decrypt(PASSWORD_KEY, EncryptedPassword);
        }

        public void SetPasswordFromPlaintext(string password)
        {
            EncryptedPassword = password == String.Empty ? String.Empty : Encryption.Encrypt(PASSWORD_KEY, password);
        }

        public bool IsEnabled()
        {
            return !String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(GetPassword());
        }
    }

    public class Services
    {
        public bool DetectExternalAddress { get; set; }
        public string CustomExternalIp { get; set; }
        public bool AuthenticationEnabled { get; set; }

        public bool BonjourEnabled { get; set; }
        public string BonjourName { get; set; }
        public bool AccessRequestEnabled { get; set; }

        public int Port { get; set; }
        public bool EnableIPv6 { get; set; }

        public string MASConnection { get; set; }
        public string TASConnection { get; set; }

        public List<User> Users { get; set; }

        public NetworkImpersonation NetworkImpersonation { get; set; }

        public Services(string path, string defaultPath)
        {
            XElement file = XElement.Load(path);

            AuthenticationEnabled = file.Element("users").Attribute("authenticationEnabled").Value == "true";

            BonjourEnabled = file.Element("bonjour").Element("enabled").Value == "true";
            BonjourName = file.Element("bonjour").Element("pcname").Value;
            AccessRequestEnabled = file.Element("accessrequest").Element("enabled").Value == "true";
            DetectExternalAddress = file.Element("externaladdress").Element("autodetect").Value == "true";
            CustomExternalIp = file.Element("externaladdress").Element("custom").Value;

            Port = Int32.Parse(file.Element("port").Value);
            EnableIPv6 = file.Element("enableIPv6").Value == "true";

            MASConnection = file.Element("connections").Element("mas").Value;
            TASConnection = file.Element("connections").Element("tas").Value;

            NetworkImpersonation = new NetworkImpersonation()
            {
                Domain = file.Element("networkImpersonation").Element("domain").Value,
                Username = file.Element("networkImpersonation").Element("username").Value,
                EncryptedPassword = file.Element("networkImpersonation").Element("password").Value,
                ReadInStreamingService = file.Element("networkImpersonation").Element("readInStreamingService").Value == "true"
            };

            Users = file.Element("users").Elements("user").Select(x => new User()
            {
                Username = x.Element("username").Value,
                EncryptedPassword = x.Element("password").Value
            }).ToList();
        }

        public bool Save() 
        {
            try
            {
                XElement file = XElement.Load(Configuration.GetPath("Services.xml"));

                file.Element("users").Attribute("authenticationEnabled").Value = AuthenticationEnabled ? "true" : "false";

                file.Element("bonjour").Element("enabled").Value = BonjourEnabled ? "true" : "false";
                file.Element("bonjour").Element("pcname").Value = BonjourName;

                file.Element("accessrequest").Element("enabled").Value = AccessRequestEnabled ? "true" : "false";

                file.Element("externaladdress").Element("autodetect").Value = DetectExternalAddress ? "true" : "false";
                file.Element("externaladdress").Element("custom").Value = CustomExternalIp;

                file.Element("port").Value = Port.ToString();
                file.Element("enableIPv6").Value = EnableIPv6 ? "true" : "false";

                file.Element("connections").Element("mas").Value = MASConnection;
                file.Element("connections").Element("tas").Value = TASConnection;

                file.Element("networkImpersonation").Element("domain").Value = NetworkImpersonation.Domain;
                file.Element("networkImpersonation").Element("username").Value = NetworkImpersonation.Username;
                file.Element("networkImpersonation").Element("password").Value = NetworkImpersonation.EncryptedPassword;
                file.Element("networkImpersonation").Element("readInStreamingService").Value = NetworkImpersonation.ReadInStreamingService ? "true" : "false";

                file.Element("users").Elements().Remove();
                foreach(User u in Users) {
                    var userElement = new XElement("user", new XElement("username", u.Username), new XElement("password", u.EncryptedPassword));
                    file.Element("users").Add(userElement);
                }

                file.Save(Configuration.GetPath("Services.xml"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to write Services.xml", ex);
                return false;
            }
        }

        public string GetServiceName()
        {
            if (!String.IsNullOrWhiteSpace(BonjourName))
            {
                return BonjourName;
            }

            try
            {
                return System.Environment.MachineName;
            }
            catch (Exception)
            {
                return "MPExtended Services";
            }
        }
    }
}
