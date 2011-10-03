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
using System.Text;
using System.ServiceModel;
using MPExtended.Services.UserSessionService;

namespace MPExtended.Services.MediaAccessService
{
    internal class UserSession
    {
        private static IUserSessionService serviceHolder = null;

        public static IUserSessionService Service
        {
            get
            {
                if (serviceHolder == null || ((ICommunicationObject)serviceHolder).State == CommunicationState.Faulted)
                {
                    serviceHolder = ChannelFactory<IUserSessionService>.CreateChannel(
                        new NetNamedPipeBinding() { MaxReceivedMessageSize = 100000000 },
                        new EndpointAddress("net.pipe://127.0.0.1/MPExtended/UserSessionService")
                    );
                }

                return serviceHolder;
            }
        }
    }
}
