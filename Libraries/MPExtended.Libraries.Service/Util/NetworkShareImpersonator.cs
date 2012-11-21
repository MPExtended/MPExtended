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
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Service.Util
{
    public class NetworkShareImpersonator : IDisposable
    {
        private Impersonator impersonator;

        public NetworkShareImpersonator(bool impersonate)
        {
            string username = Configuration.Services.NetworkImpersonation.Username;
            string password = Configuration.Services.NetworkImpersonation.GetPassword();
            if (impersonate && Configuration.Services.NetworkImpersonation.IsEnabled())
            {
                try
                {
                    impersonator = new Impersonator(Configuration.Services.NetworkImpersonation.Domain, username, password);
                }
                catch (Exception e)
                {
                    Log.Warn("Failed to impersonate {0} -> {1}", username, e.Message);
                    impersonator = null;
                }
            }
        }

        public NetworkShareImpersonator()
            : this(true)
        {
        }

        public void Dispose()
        {
            if (impersonator != null)
            {
                impersonator.Dispose();
                impersonator = null;
            }
        }
    }
}
