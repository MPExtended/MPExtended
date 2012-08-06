﻿#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Threading.Tasks;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.ConfigurationContracts;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MetaService.Interfaces;
using MPExtended.Services.UserSessionService.Interfaces;
using MPExtended.Libraries.Service.MpConnection;
using System.Threading;

namespace MPExtended.Services.MetaService
{
    internal class AccessRequests
    {
        private Dictionary<string, WebAccessRequestResponse> requests = new Dictionary<string, WebAccessRequestResponse>();
        private Dictionary<string, Task<bool>> askUserTasks = new Dictionary<string, Task<bool>>();
        private Dictionary<string, CancellationTokenSource> cancelTokens = new Dictionary<string, CancellationTokenSource>();
        public WebAccessRequestResponse CreateAccessRequest(string clientName)
        {
            // generate the request
            string ip = WCFUtil.GetClientIPAddress();
            string token = RandomGenerator.GetRandomString(40);
            WebAccessRequestResponse request = new WebAccessRequestResponse()
            {
                ClientName = clientName,
                IsAllowed = false,
                UserHasResponded = false,
                Token = token
            };
            requests[token] = request;
            Log.Info("Received access request from {0} (claims to be client {1}), gave it token {2}", ip, clientName, token);

            CancellationTokenSource cancelToken = new CancellationTokenSource();
            cancelTokens[token] = cancelToken;

            // ask the user
            askUserTasks[token] = Task.Factory.StartNew(delegate()
            {
                String result = null;
                // TODO: maybe 
                if (Mediaportal.IsMediaPortalRunning() && WifiRemote.IsInstalled)
                {
                    //go through WifiRemote when MP is open and WifiRemote is installed
                    result = RequestAccessThroughWifiRemote(clientName, ip);
                }
                else
                {
                    //if we can't use WifiRemote, try to get the users response via USS (the configuration tool)
                    result = RequestAccessThroughPrivateUSS(clientName, ip);
                }
                Log.Debug("Got user response to access request with token {0}: {1}", token, result);

                // set the necessary flags
                lock (requests[token])
                {
                    requests[token].IsAllowed = result != null;
                    requests[token].UserHasResponded = true;
                    if (result != null)
                    {
                        User user = Configuration.Services.Users.Where(x => x.Username.Equals(result)).First();

                        if (user != null)
                        {
                            Log.Info("Sending account {0} in response to access request {1}", user.Username, token);
                            requests[token].Username = user.Username;
                            requests[token].Password = user.GetPassword();
                        }
                        else
                        {
                            Log.Warn("No user available for username {0}", result);
                        }
                    }
                }
                return true;
            }, cancelToken.Token);

            // return the token to the client
            return request;
        }

        private String RequestAccessThroughWifiRemote(string clientName, string ip)
        {

            User auth = WifiRemote.GetAuthentication();
            WifiRemoteClient client = new WifiRemoteClient(auth, "localhost", WifiRemote.Port);
            string result = null;
            try
            {
                client.Connect();
                bool loggedIn = false;

                while (!client.ConnectionFailed)
                {
                    if (!loggedIn && client.LoggedIn)
                    {
                        client.SendRequestAccessDialog(clientName, ip, User.GetStringArray(Configuration.Services.Users));
                        loggedIn = true;
                    }

                    if (client.LatestDialogResult != null)
                    {
                        //User accepted or denied the request
                        result = client.LatestDialogResult.SelectedOption;
                        break;
                    }
                    Thread.Sleep(200);
                }

                client.Disconnect();
            }
            catch (AggregateException)
            {
                if (client != null)
                {
                    client.CancelRequestAccessDialog();
                    client.Disconnect();
                }
            }
            return result;
        }

        public WebAccessRequestResponse GetAccessRequestStatus(string token)
        {
            return requests[token];
        }

        public WebAccessRequestResponse GetAccessRequestStatusBlocking(string token, int timeout)
        {
            askUserTasks[token].Wait(TimeSpan.FromSeconds(timeout));
            return requests[token];
        }

        private String RequestAccessThroughPrivateUSS(string client, string ip)
        {
            IPrivateUserSessionService channel = null;
            try
            {
                // create channel
                NetTcpBinding binding = new NetTcpBinding()
                {
                    MaxReceivedMessageSize = 100000000,
                    ReceiveTimeout = new TimeSpan(1, 0, 0),
                    SendTimeout = new TimeSpan(1, 0, 0),
                };
                binding.ReliableSession.Enabled = true;
                binding.ReliableSession.Ordered = true;
                channel = ChannelFactory<IPrivateUserSessionService>.CreateChannel(
                    binding,
                    new EndpointAddress("net.tcp://localhost:9751/MPExtended/UserSessionServicePrivate")
                );

                // request access
                String result = channel.RequestAccess(client, ip, User.GetStringArray(Configuration.Services.Users));

                // close channel
                (channel as ICommunicationObject).Close();
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to request access for client {0} at {1} through USS", client, ip), ex);
                return null;
            }
        }

        public bool CancelAccessRequest(string token)
        {
            //TODO: find a way to cancel the requests
            //requests.Remove(token);
            //askUserTasks.Remove(token);
            //cancelTokens.Remove(token);

            return false;
        }
    }
}
