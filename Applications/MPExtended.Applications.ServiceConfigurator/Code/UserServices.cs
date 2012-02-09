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
using System.ServiceModel;
using MPExtended.Libraries.Service;
using MPExtended.Services.UserSessionService;
using MPExtended.Services.UserSessionService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal static class UserServices
    {
        private enum ConnectionMode
        {
            SelfHosting,
            RemoteConnection,
            Failure
        }

        private static ConnectionMode type;

        private static ServiceHost ussHost;
        private static ServiceHost privateHost;
        private static IUserSessionService ussInstance;
        private static IPrivateUserSessionService privateInstance;
        private static IUserSessionService ussConnection;
        private static IPrivateUserSessionService privateConnection;

        public static IUserSessionService USS
        {
            get
            {
                if (type == ConnectionMode.RemoteConnection)
                {
                    return CreateChannel<IUserSessionService>(ref ussConnection, "net.tcp://localhost:9750/MPExtended/UserSessionServiceImplementation");
                }
                else if (type == ConnectionMode.SelfHosting)
                {
                    return ussInstance;
                }
                else
                {
                    return null;
                }
            }
        }

        public static IPrivateUserSessionService Private
        {
            get
            {
                if (type == ConnectionMode.RemoteConnection)
                {
                    return CreateChannel<IPrivateUserSessionService>(ref privateConnection, "net.tcp://localhost:9751/MPExtended/UserSessionServicePrivate");
                }
                else if (type == ConnectionMode.SelfHosting)
                {
                    return privateInstance;
                }
                else
                {
                    return null;
                }
            }
        }

        public static void Setup(bool host)
        {
            if (!host)
            {
                type = ConnectionMode.RemoteConnection;
                return;
            }

            try
            {
                ussHost = new ServiceHost(typeof(UserSessionService));
                privateHost = new ServiceHost(typeof(PrivateUserSessionService));

                Log.Debug("Opening ServiceHost...");
                ussHost.Open();
                privateHost.Open();

                Log.Info("UserSessionService started...");
                type = ConnectionMode.SelfHosting;
                ussInstance = new UserSessionService();
                privateInstance = new PrivateUserSessionService();
            }
            catch (AddressAlreadyInUseException)
            {
                Log.Info("Address for UserSessionService is already in use");
                type = ConnectionMode.RemoteConnection;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to start service", ex);
                type = ConnectionMode.Failure;
            }
        }

        private static T CreateChannel<T>(ref T original, string address)
        {
            // if channel is working, just return it
            if (original != null && ((ICommunicationObject)original).State != CommunicationState.Faulted)
            {
                return original;
            }

            // try to close a faulted channel
            if (original != null)
            {
                try
                {
                    ((ICommunicationObject)original).Close(TimeSpan.FromMilliseconds(500));
                }
                catch (Exception)
                {
                    // oops.
                }
            }

            // recreate channel
            NetTcpBinding binding = new NetTcpBinding()
            {
                MaxReceivedMessageSize = 100000000,
                ReceiveTimeout = new TimeSpan(0, 0, 5),
                SendTimeout = new TimeSpan(0, 0, 5),
            };
            binding.ReliableSession.Enabled = true;
            binding.ReliableSession.Ordered = true;

            original = ChannelFactory<T>.CreateChannel(
                binding,
                new EndpointAddress(address)
            );

            return original;
        }
    }
}
