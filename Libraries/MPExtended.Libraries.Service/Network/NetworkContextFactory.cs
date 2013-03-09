#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Network
{
    public static class NetworkContextFactory
    {
        public static INetworkContext Create()
        {
            return Configuration.Services.NetworkImpersonation.IsEnabled() ? 
                CreateImpersonationContext() : 
                CreateLocalContext();
        }

        public static INetworkContext Create(string path)
        {
            return Configuration.Services.NetworkImpersonation.IsEnabled() && PathUtil.MightBeOnNetworkDrive(path) ? 
                CreateImpersonationContext() : 
                CreateLocalContext();
        }

        public static INetworkContext Create(bool impersonate)
        {
            return Configuration.Services.NetworkImpersonation.IsEnabled() && impersonate ?
                CreateImpersonationContext() :
                CreateLocalContext();
        }

        public static INetworkContext CreateLocalContext()
        {
            return new LocalMachineContext();
        }

        public static INetworkContext CreateImpersonationContext()
        {
            return new ImpersonationContext();
        }
    }
}
