#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    [XmlType(Namespace = "http://mpextended.github.io/schema/config/Users/1")]
    public class User
    {
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }

        // For later usage, such as saving per-user settings or whatever we'll invent. 
        public ConfigDictionary CustomConfiguration { get; set; }

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

    [XmlRoot(Namespace = "http://mpextended.github.io/schema/config/Users/1")]
    public class Authentication
    {
        public bool Enabled { get; set; }
        public bool UnauthorizedStreams { get; set; }

        [XmlArray(Namespace = "http://mpextended.github.io/schema/config/Users/1", ElementName = "Users")]
        [XmlArrayItem(ElementName = "User")]
        public List<User> Users { get; set; }

        public Authentication()
        {
            Users = new List<User>();
        }
    }
}
