#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Security.Cryptography;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public class XMLMembershipProvider : MembershipProvider
    {
        private struct User
        {
            public string passwordHash;
            public MembershipUser user;
        }

        private NameValueCollection config;
        private Dictionary<string, User> users;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (name == null)
                name = this.GetType().Name;
            base.Initialize(name, config);
            this.config = config;
        }

        private string GetConfigValue(string key, string defaultValue)
        {
            return String.IsNullOrEmpty(config[key]) ? defaultValue : config[key];
        }

        #region Properties
        public override string ApplicationName
        {
            get { return GetConfigValue("applicationName", System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath); }
            set { config["applicationName"] = value; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return Convert.ToBoolean(GetConfigValue("requiresUniqueEmail", "true")); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return Convert.ToInt32(GetConfigValue("maxInvalidPasswordAttempts", "5")); }
        }

        public override int MinRequiredPasswordLength
        {
            get { return Convert.ToInt32(GetConfigValue("maxInvalidPasswordAttempts", "5")); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return GetConfigValue("passwordStrengthRegularExpression", ""); }
        }

        // not actually used
        public override int PasswordAttemptWindow
        {
            get { return Convert.ToInt32(GetConfigValue("maxInvalidPasswordAttempts", "10")); }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return Convert.ToInt32(GetConfigValue("maxInvalidPasswordAttempts", "5")); }
        }

        // disable some advanced password operations for now
        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Data handling
        protected void ReadData()
        {
            if (users != null)
                return;

            users = new Dictionary<string, User>();
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "config.users.xml");
            } catch (System.IO.FileNotFoundException)
            {
                return;
            }
            XmlNodeList readUsers = doc.SelectNodes("/webmediaportal/users/user");
            foreach (XmlNode user in readUsers)
            {
                users[user.ChildNodes.GetFirstNode("username").InnerText] = new User()
                {
                    passwordHash = user.ChildNodes.GetFirstNode("password").InnerText,
                    user = new MembershipUser(
                        this.Name, 
                        user.ChildNodes.GetFirstNode("username").InnerText,
                        null,
                        user.ChildNodes.GetFirstNode("email").InnerText,
                        user.ChildNodes.GetFirstNode("passwordQuestion").InnerText,
                        user.ChildNodes.GetFirstNode("comment").InnerText,
                        true,
                        false,
                        DateTime.Parse(user.ChildNodes.GetFirstNode("creationDate").InnerText),
                        DateTime.Parse(user.ChildNodes.GetFirstNode("lastLoginDate").InnerText),
                        DateTime.Parse(user.ChildNodes.GetFirstNode("lastActivity").InnerText),
                        DateTime.Parse(user.ChildNodes.GetFirstNode("lastPasswordChangedDate").InnerText),
                        new DateTime()
                    )
                };
            }
        }

        // TODO: this needs to be done in the an appropriate config file
        protected void WriteData()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("webmediaportal");
            XmlNode users = doc.CreateElement("users");
            foreach (KeyValuePair<string, User> item in this.users)
            {
                User u = item.Value;
                XmlNode user = doc.CreateElement("user");
                AddChild(doc, user, "password", u.passwordHash);
                AddChild(doc, user, "comment", u.user.Comment);
                AddChild(doc, user, "creationDate", u.user.CreationDate);
                AddChild(doc, user, "email", u.user.Email);
                AddChild(doc, user, "isApproved", u.user.IsApproved ? "true" : "false");
                AddChild(doc, user, "lastActivity", u.user.LastActivityDate);
                AddChild(doc, user, "lastLoginDate", u.user.LastLoginDate);
                AddChild(doc, user, "lastPasswordChangedDate", u.user.LastPasswordChangedDate);
                AddChild(doc, user, "passwordQuestion", u.user.PasswordQuestion);
                AddChild(doc, user, "username", u.user.UserName);
                users.AppendChild(user);
            }
            root.AppendChild(users);
            doc.AppendChild(root);
            doc.Save(AppDomain.CurrentDomain.BaseDirectory + "config.users.xml");
        }

        private void AddChild(XmlDocument doc, XmlNode parent, string key, string value)
        {
            XmlNode node = doc.CreateElement(key);
            node.InnerText = value;
            parent.AppendChild(node);
        }

        private void AddChild(XmlDocument doc, XmlNode parent, string key, DateTime value)
        {
            AddChild(doc, parent, key, value.ToString("s"));
        }

        private string GenerateSHA1Hash(string input)
        {
            SHA1Managed hashprovider = new SHA1Managed();
            System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
            byte[] bytes = encoder.GetBytes(input);
            byte[] hash = hashprovider.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
        #endregion

        #region Action methods
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            ReadData();
            if (!ValidateUser(username, oldPassword))
                return false;

            User u = users[username];
            u.passwordHash = GenerateSHA1Hash(newPassword);
            users[username] = u;

            WriteData();
            return true;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            ReadData();
            if (users.ContainsKey(username))
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "") {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            User u;
            u.passwordHash = GenerateSHA1Hash(password);
            u.user = new MembershipUser(this.Name, username, providerUserKey, email, passwordQuestion, "", isApproved, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, new DateTime());
            users.Add(username, u);

            WriteData();
            status = MembershipCreateStatus.Success;
            return u.user;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            ReadData();
            if (users.ContainsKey(username))
                users.Remove(username);
            WriteData();
            return true;
        }

        public override bool UnlockUser(string userName)
        {
            // should never be called as there is no way to lock users
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            ReadData();
            User u = users[user.UserName];
            u.user = user;
            users[user.UserName] = u;
            WriteData();
        }
        #endregion

        #region Find users
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => u.Value.user.Email == emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => u.Value.user.UserName == usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => true, pageIndex, pageSize, out totalRecords);
        }

        private MembershipUserCollection FindUsersByCondition(Func<KeyValuePair<string, User>, bool> condition, int pageIndex, int pageSize, out int totalRecords)
        {
            ReadData();
            MembershipUserCollection ret = new MembershipUserCollection();
            List<MembershipUser> valid = users.Where(condition).Select(u => u.Value.user).ToList();
            totalRecords = valid.Count;
            valid.Skip(pageIndex * pageSize).Take(pageSize).ToList().ForEach(u => ret.Add(u));
            return ret;
        }

        public override int GetNumberOfUsersOnline()
        {
            return 0;
        }

        private User? GetUser(Func<KeyValuePair<string, User>, bool> condition, bool userIsOnline)
        {
            ReadData();
            User user;
            try
            {
                user = users.Where(condition).First().Value;
            } catch (Exception)
            {
                // probably nothing found
                return null;
            }
            if (userIsOnline)
            {
                User u = users[user.user.UserName];
                u.user.LastActivityDate = DateTime.Now;
                WriteData();
            }
            return user;
        }

        private MembershipUser GetMembershipUser(Func<KeyValuePair<string, User>, bool> condition, bool userIsOnline)
        {
            User? u = GetUser(condition, userIsOnline);
            return u == null ? null : u.Value.user;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return GetMembershipUser(u => u.Value.user.UserName == username, userIsOnline);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return GetMembershipUser(u => u.Value.user.ProviderUserKey == providerUserKey, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            MembershipUser user = GetMembershipUser(u => u.Value.user.Email == email, false);
            return user == null ? "" : user.UserName;
        }

        public override bool ValidateUser(string username, string password)
        {
            return GetUser(u => u.Value.user.UserName == username && u.Value.passwordHash == GenerateSHA1Hash(password), false) != null;
        }
        #endregion
    }
}