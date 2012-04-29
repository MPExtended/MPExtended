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
                {
                    NetTcpBinding binding = new NetTcpBinding()
                    {
                        MaxReceivedMessageSize = 100000000,
                        ReceiveTimeout = new TimeSpan(0, 0, 10),
                        SendTimeout = new TimeSpan(0, 0, 10),
                    };
                    binding.ReliableSession.Enabled = true;
                    binding.ReliableSession.Ordered = true;

                    proxyChannel = ChannelFactory<IUserSessionService>.CreateChannel(
                        binding,
                        new EndpointAddress("net.tcp://localhost:9750/MPExtended/UserSessionServiceImplementation")
                    );
                }

                return proxyChannel;
            }
        }

        public WebResult TestConnection()
        {
            try
            {
                return Proxy.TestConnection();
            }
            catch (Exception)
            {
                // don't even log them, they're too much noice
                //Log.Trace("No connection to user session service", e);
                return new WebResult(false);
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

        public WebResult IsMediaPortalRunning()
        {
            return Proxy.IsMediaPortalRunning();
        }

        public WebResult StartMediaPortal()
        {
            return Proxy.StartMediaPortal();
        }

        public WebResult StartMediaPortalBlocking()
        {
            return Proxy.StartMediaPortalBlocking();
        }

        public WebResult SetMediaPortalForeground()
        {
            return Proxy.SetMediaPortalForeground();
        }

        public WebResult SetPowerMode(WebPowerMode powerMode)
        {
            return Proxy.SetPowerMode(powerMode);
        }

        public WebResult CloseMediaPortal()
        {
            return Proxy.CloseMediaPortal();
        }
    }
}
