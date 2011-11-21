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
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public class XMLMembershipProvider : MembershipProvider
    {
        private struct UserHolder
        {
            public string PasswordHash { get; set; }
            public MembershipUser User { get; set; }
        }

        private NameValueCollection config;
        private Dictionary<string, UserHolder> users;

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

            XElement file = XElement.Load(Configuration.GetPath("WebMediaPortalUsers.xml"));
            users = new Dictionary<string, UserHolder>();

            users = file.Element("users").Elements("user").Select(u => new UserHolder()
            {
                PasswordHash = u.Element("password").Value,
                User = new MembershipUser(
                    this.Name,
                    u.Element("username").Value,
                    null,
                    u.Element("email").Value,
                    u.Element("passwordQuestion").Value,
                    u.Element("comment").Value,
                    true,
                    false,
                    DateTime.Parse(u.Element("creationDate").Value),
                    DateTime.Parse(u.Element("lastLoginDate").Value),
                    DateTime.Parse(u.Element("lastActivity").Value),
                    DateTime.Parse(u.Element("lastPasswordChangedDate").Value),
                    new DateTime()                    
                )
            }).ToDictionary(x => x.User.UserName, x => x);
        }

        protected void WriteData()
        {
            string path = Configuration.GetPath("WebMediaPortalUsers.xml");
            XElement file = XElement.Load(path);
            file.Element("users").Elements().Remove();
            foreach (KeyValuePair<string, UserHolder> item in this.users)
            {
                file.Element("users").Add(new XElement("user",
                    new XElement("password", item.Value.PasswordHash),
                    new XElement("comment", item.Value.User.Comment),
                    new XElement("creationDate", item.Value.User.CreationDate),
                    new XElement("email", item.Value.User.Email),
                    new XElement("isApproved", item.Value.User.IsApproved ? "true" : "false"),
                    new XElement("lastActivity", item.Value.User.LastActivityDate),
                    new XElement("lastLoginDate", item.Value.User.LastLoginDate),
                    new XElement("lastPasswordChangedDate", item.Value.User.LastPasswordChangedDate),
                    new XElement("passwordQuestion", item.Value.User.PasswordQuestion),
                    new XElement("username", item.Value.User.UserName)
                ));
            }
            file.Save(path);
        }

        private string GeneratePasswordHash(string input)
        {
            SHA1Managed hashprovider = new SHA1Managed();
            byte[] bytes = Encoding.ASCII.GetBytes(input);
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

            UserHolder u = users[username];
            u.PasswordHash = GeneratePasswordHash(newPassword);
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

            UserHolder u = new UserHolder();
            u.PasswordHash = GeneratePasswordHash(password);
            u.User = new MembershipUser(this.Name, username, providerUserKey, email, passwordQuestion, "", isApproved, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now, new DateTime());
            users.Add(username, u);

            WriteData();
            status = MembershipCreateStatus.Success;
            return u.User;
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
            UserHolder u = users[user.UserName];
            u.User = user;
            users[user.UserName] = u;
            WriteData();
        }
        #endregion

        #region Find users
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => u.Value.User.Email == emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => u.Value.User.UserName == usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => true, pageIndex, pageSize, out totalRecords);
        }

        private MembershipUserCollection FindUsersByCondition(Func<KeyValuePair<string, UserHolder>, bool> condition, int pageIndex, int pageSize, out int totalRecords)
        {
            ReadData();
            MembershipUserCollection ret = new MembershipUserCollection();
            List<MembershipUser> valid = users.Where(condition).Select(u => u.Value.User).ToList();
            totalRecords = valid.Count;
            valid.Skip(pageIndex * pageSize).Take(pageSize).ToList().ForEach(u => ret.Add(u));
            return ret;
        }

        public override int GetNumberOfUsersOnline()
        {
            return 0;
        }

        private UserHolder? GetUser(Func<KeyValuePair<string, UserHolder>, bool> condition, bool userIsOnline)
        {
            ReadData();
            UserHolder user;
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
                UserHolder u = users[user.User.UserName];
                u.User.LastActivityDate = DateTime.Now;
                WriteData();
            }
            return user;
        }

        private MembershipUser GetMembershipUser(Func<KeyValuePair<string, UserHolder>, bool> condition, bool userIsOnline)
        {
            UserHolder? u = GetUser(condition, userIsOnline);
            return u == null ? null : u.Value.User;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return GetMembershipUser(u => u.Value.User.UserName == username, userIsOnline);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return GetMembershipUser(u => u.Value.User.ProviderUserKey == providerUserKey, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            MembershipUser user = GetMembershipUser(u => u.Value.User.Email == email, false);
            return user == null ? "" : user.UserName;
        }

        public override bool ValidateUser(string username, string password)
        {
            return GetUser(u => u.Value.User.UserName == username && u.Value.PasswordHash == GeneratePasswordHash(password), false) != null;
        }
        #endregion
    }
}