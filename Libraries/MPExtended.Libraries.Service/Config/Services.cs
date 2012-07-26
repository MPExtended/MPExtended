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

namespace MPExtended.Libraries.Service.Config
{
    public class User
    {
        private const string PASSWORD_KEY = "MPExtended Password Key"; // And here all our security goes out of the window: the key is on the internet.

        public string Username { get; set; }
        public string EncryptedPassword { get; set; }

        public User()
        {
        }

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

        public NetworkImpersonation()
        {
        }

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
        public Services()
        {
        }

        public bool AuthenticationEnabled { get; set; }

        public bool BonjourEnabled { get; set; }
        public string BonjourName { get; set; }

        public int Port { get; set; }
        public bool EnableIPv6 { get; set; }

        public string MASConnection { get; set; }
        public string TASConnection { get; set; }

        public List<User> Users { get; set; }

        public NetworkImpersonation NetworkImpersonation { get; set; }

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
