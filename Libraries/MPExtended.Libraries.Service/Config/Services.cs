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
using System.Runtime.Serialization;
using System.Text;
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Config
{
    [DataContract(Name = "User", Namespace = "http://mpextended.github.com/schema/config/Services/1")]
    public class User
    {
        private const string PASSWORD_KEY = "MPExtended Password Key"; // And here all our security goes out of the window: the key is on the internet.

        [DataMember]
        public string Username { get; set; }
        [DataMember]
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

    [DataContract(Name = "NetworkImpersonation", Namespace = "http://mpextended.github.com/schema/config/Services/1")]
    public class NetworkImpersonation
    {
        private const string PASSWORD_KEY = "MPExtended Impersonation Password"; // And here all our security goes out of the window: the key is on the internet.

        [DataMember]
        public string Domain { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string EncryptedPassword { get; set; }
        [DataMember]
        public bool ReadInStreamingService { get; set; }

        public NetworkImpersonation()
        {
            ReadInStreamingService = true;
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

    [DataContract(Name = "Services", Namespace = "http://mpextended.github.com/schema/config/Services/1")]
    public class Services
    {
        [DataMember]
        public bool AuthenticationEnabled { get; set; }

        [DataMember]
        public bool BonjourEnabled { get; set; }
        [DataMember]
        public string BonjourName { get; set; }

        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public bool EnableIPv6 { get; set; }

        [DataMember]
        public string MASConnection { get; set; }
        [DataMember]
        public string TASConnection { get; set; }

        [DataMember]
        public List<User> Users { get; set; }

        [DataMember]
        public NetworkImpersonation NetworkImpersonation { get; set; }

        public Services()
        {
            Port = 4322;
            MASConnection = "auto://127.0.0.1:4322";
            TASConnection = "auto://127.0.0.1:4322";
            Users = new List<User>();
            NetworkImpersonation = new NetworkImpersonation();
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
