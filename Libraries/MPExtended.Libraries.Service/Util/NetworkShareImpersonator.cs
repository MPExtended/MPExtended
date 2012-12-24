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
using MPExtended.Libraries.Service.Network;

namespace MPExtended.Libraries.Service.Util
{
    public class NetworkShareImpersonator : IDisposable
    {
        private static MappedDriveConverter mdcCache = null;

        private Impersonator impersonator;

        public NetworkShareImpersonator()
        {
            if (Configuration.Services.NetworkImpersonation.IsEnabled())
            {
                string username = Configuration.Services.NetworkImpersonation.Username;
                try
                {
                    impersonator = new Impersonator(Configuration.Services.NetworkImpersonation.Domain, username, Configuration.Services.NetworkImpersonation.GetPassword());
                }
                catch (Exception e)
                {
                    Log.Warn("Failed to impersonate {0} -> {1}", username, e.Message);
                    impersonator = null;
                }
            }
        }

        public string RewritePath(string path)
        {
            if (impersonator == null)
                return path;
            if (mdcCache == null)
                mdcCache = new MappedDriveConverter();
            return mdcCache.ConvertPathToUnc(Configuration.Services.NetworkImpersonation.Domain, Configuration.Services.NetworkImpersonation.Username, path);
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
