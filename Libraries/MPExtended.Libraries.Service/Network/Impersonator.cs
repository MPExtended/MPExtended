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
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace MPExtended.Libraries.Service.Network
{
    internal class Impersonator : IDisposable
    {
        [DllImport("advapi32.dll")]
        private static extern int LogonUserA(string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;

        private WindowsImpersonationContext impersonationContext;

        public Impersonator(string domain, string username, string password)
        {
            if (String.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be empty", "username");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty", "password");

            if (!DoImpersonation(domain, username, password))
            {
                throw new Exception("Failed to impersonate");
            }
        }

        public void Dispose()
        {
            UndoImpersonation();
        }

        private bool DoImpersonation(string domain, string username, string password)
        {
            WindowsIdentity identity;
            IntPtr token = IntPtr.Zero;
            IntPtr duplicatedToken = IntPtr.Zero;

            try
            {
                if (!RevertToSelf())
                    return false;

                if (LogonUserA(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) == 0)
                    return false;

                if (DuplicateToken(token, 2, ref duplicatedToken) == 0)
                    return false;

                identity = new WindowsIdentity(duplicatedToken);
                impersonationContext = identity.Impersonate();
                if (impersonationContext == null)
                    return false;

                // succeeded
                return true;
            }
            finally
            {
                // cleanup
                if (token != IntPtr.Zero)
                    CloseHandle(token);

                if (duplicatedToken != IntPtr.Zero)
                    CloseHandle(duplicatedToken);
            }
        }

        private void UndoImpersonation()
        {
            impersonationContext.Undo();
        }
    }
}