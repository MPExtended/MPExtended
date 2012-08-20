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
using System.ServiceModel;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service.WCF;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.UserSessionService.Interfaces;

namespace MPExtended.Services.UserSessionService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class UserSessionProxyService : IUserSessionService
    {
        private IUserSessionService proxyChannel;
        private IUserSessionService Proxy
        {
            get
            {
                bool recreateChannel = false;
                if (proxyChannel == null)
                {
                    recreateChannel = true;
                } 
                else if(((ICommunicationObject)proxyChannel).State == CommunicationState.Faulted)
                {
                    try
                    {
                        ((ICommunicationObject)proxyChannel).Close(TimeSpan.FromMilliseconds(500));
                    }
                    catch (Exception)
                    {
                        // oops. 
                    }

                    recreateChannel = true;
                }

                if (recreateChannel)
                    proxyChannel = new ClientFactory<IUserSessionService>().CreateLocalTcpConnection(9750, "MPExtended/UserSessionServiceImplementation");

                return proxyChannel;
            }
        }

        public WebBoolResult TestConnection()
        {
            try
            {
                return Proxy.TestConnection();
            }
            catch (Exception)
            {
                // don't even log them, they're too much noice
                //Log.Trace("No connection to user session service", e);
                return new WebBoolResult(false);
            }
        }

        public WebUserServiceDescription GetServiceDescription()
        {
            return new WebUserServiceDescription()
            {
                ApiVersion = UserSessionService.API_VERSION,
                ServiceVersion = VersionUtil.GetVersionName(),
                IsAvailable = TestConnection()
            };
        }

        public WebBoolResult IsMediaPortalRunning()
        {
            return Proxy.IsMediaPortalRunning();
        }

        public WebBoolResult StartMediaPortal()
        {
            return Proxy.StartMediaPortal();
        }

        public WebBoolResult StartMediaPortalBlocking()
        {
            return Proxy.StartMediaPortalBlocking();
        }

        public WebBoolResult SetMediaPortalForeground()
        {
            return Proxy.SetMediaPortalForeground();
        }

        public WebBoolResult SetPowerMode(WebPowerMode powerMode)
        {
            return Proxy.SetPowerMode(powerMode);
        }

        public WebBoolResult CloseMediaPortal()
        {
            return Proxy.CloseMediaPortal();
        }
    }
}
