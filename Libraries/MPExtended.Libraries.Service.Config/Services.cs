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
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    [XmlType(Namespace = "http://mpextended.github.com/schema/config/Services/1")]
    public class User
    {
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
            return Transformations.Decrypt(EncryptedPassword);
        }

        public void SetPasswordFromPlaintext(string password)
        {
            EncryptedPassword = Transformations.Encrypt(password);
        }
    }

    [XmlType(Namespace="http://mpextended.github.com/schema/config/Services/1")]
    public class NetworkImpersonation
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public bool ReadInStreamingService { get; set; }

        public NetworkImpersonation()
        {
            ReadInStreamingService = true;
        }

        public string GetPassword()
        {
            return EncryptedPassword == String.Empty ? String.Empty : Transformations.Decrypt(EncryptedPassword);
        }

        public void SetPasswordFromPlaintext(string password)
        {
            EncryptedPassword = password == String.Empty ? String.Empty : Transformations.Encrypt(password);
        }

        public bool IsEnabled()
        {
            return !String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(GetPassword());
        }
    }

    [XmlRoot(Namespace="http://mpextended.github.com/schema/config/Services/1")]
    public class Services
    {
        public bool AuthenticationEnabled { get; set; }

        public bool BonjourEnabled { get; set; }
        public string BonjourName { get; set; }


        public int Port { get; set; }
        public bool EnableIPv6 { get; set; }

        public string MASConnection { get; set; }
        public string TASConnection { get; set; }

        public List<User> Users { get; set; }

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
