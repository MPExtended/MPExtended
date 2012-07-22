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
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.ConfigurationContracts;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    internal class ServiceMembershipProvider : MembershipProvider
    {
        private NameValueCollection config;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (name == null)
                name = this.GetType().Name;
            base.Initialize(name, config);
            this.config = config;
        }

        #region Properties
        public override string ApplicationName
        {
            get { return String.IsNullOrEmpty(config["applicationName"]) ? System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath : config["applicationName"]; }
            set { config["applicationName"] = value; }
        }

        // disable all other properties
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Encrypted; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 0; }
        }

        public override int PasswordAttemptWindow
        {
            get { return 0; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return 0; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return ""; }
        }

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
        #endregion

        #region Disabled methods
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

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            return 0;
        }
        #endregion

        #region Find users
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => u.Username == usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return FindUsersByCondition(u => true, pageIndex, pageSize, out totalRecords);
        }

        private MembershipUserCollection FindUsersByCondition(Func<User, bool> condition, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection ret = new MembershipUserCollection();
            List<MembershipUser> valid = Configuration.Services.Users.Where(condition).Select(u => GetMembershipUser(u, false)).ToList();
            totalRecords = valid.Count;
            valid.Skip(pageIndex * pageSize).Take(pageSize).ToList().ForEach(u => ret.Add(u));
            return ret;
        }

        private MembershipUser GetMembershipUser(User user, bool userIsOnline)
        {
            return new MembershipUser(
                this.Name,
                user.Username,
                null,
                null,
                null,
                null,
                true,
                false,
                DateTime.Now,
                DateTime.Now,
                DateTime.Now,
                DateTime.Now,
                DateTime.Now
            );
        }

        private MembershipUser GetMembershipUser(Func<User, bool> condition, bool userIsOnline)
        {
            try
            {
                User u = Configuration.Services.Users.Where(condition).First();
                return GetMembershipUser(u, userIsOnline);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return GetMembershipUser(u => u.Username == username, userIsOnline);
        }

        public override bool ValidateUser(string username, string password)
        {
            var list = Configuration.Services.Users.Where(u => u.Username == username);
            return list.Count() > 0 && list.First().ValidatePassword(password);
        }
        #endregion
    }
}