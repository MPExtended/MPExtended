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
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Services.UserSessionService.Interfaces;

namespace MPExtended.Services.UserSessionService
{
    public class UserSessionProxyService : IUserSessionService
    {
        private IUserSessionService proxy;

        public UserSessionProxyService()
        {
            proxy = ChannelFactory<IUserSessionService>.CreateChannel(
                new NetNamedPipeBinding() { 
                    MaxReceivedMessageSize = 100000000,  
                    ReceiveTimeout = new TimeSpan(0, 0, 5),
                    SendTimeout = new TimeSpan(0, 0, 5),
                },
                new EndpointAddress("net.pipe://localhost/MPExtended/UserSessionServiceImplementation")
            );
        }

        public WebResult TestConnection()
        {
            try
            {
                return proxy.TestConnection();
            }
            catch (Exception)
            {
                // don't even log them, they're too much noice
                //Log.Trace("No connection to user session service", e);
                return new WebResult(false);
            }
        }

        public WebResult IsMediaPortalRunning()
        {
            return proxy.IsMediaPortalRunning();
        }

        public WebResult StartMediaPortal()
        {
            return proxy.StartMediaPortal();
        }

        public WebResult StartMediaPortalBlocking()
        {
            return proxy.StartMediaPortalBlocking();
        }

        public WebResult SetPowerMode(WebPowerMode powerMode)
        {
            return proxy.SetPowerMode(powerMode);
        }

        public WebResult CloseMediaPortal()
        {
            return proxy.CloseMediaPortal();
        }
    }
}
