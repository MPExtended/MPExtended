#region Copyright (C) 2012 MPExtended
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
                if (Mediaportal.IsMediaPortalRunning() && WifiRemote.IsInstalled)
                {
                    //go through WifiRemote when MP is open and WifiRemote is installed
                    result = RequestAccessThroughWifiRemote(clientName, ip, cancelToken);
                }
                else
                {
                    //if we can't use WifiRemote, try to get the users response via USS (the configuration tool)
                    result = RequestAccessThroughPrivateUSS(token, clientName, ip, cancelToken);
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
            }, cancelToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // return the token to the client
            return request;
        }

        private String RequestAccessThroughWifiRemote(string clientName, string ip, CancellationTokenSource cancelToken)
        {
            User auth = WifiRemote.GetAuthentication();
            WifiRemoteClient client = new WifiRemoteClient(auth, "localhost", WifiRemote.Port);
            string result = null;

            client.Connect();
            bool loggedIn = false;

            while (!client.ConnectionFailed)
            {
                if (!loggedIn && client.LoggedIn)
                {
                    client.SendRequestAccessDialog(clientName, ip, Configuration.Services.Users.Select(x => x.Username).ToList());
                    loggedIn = true;
                }

                if (cancelToken.IsCancellationRequested)
                {
                    client.CancelRequestAccessDialog();
                }

                if (client.LatestDialogResult != null)
                {
                    //User accepted or denied the request
                    result = client.LatestDialogResult.SelectedOption;
                    break;
                }
                Thread.Sleep(500);
            }

            client.Disconnect();
            return result;
        }

        public WebAccessRequestResponse GetAccessRequestStatus(string token)
        {
            if (requests.ContainsKey(token))
            {
                return requests[token];
            }
            else
            {
                Log.Warn("No access request for token {0}", token);
                return null;
            }
        }

        public WebAccessRequestResponse GetAccessRequestStatusBlocking(string token, int timeout)
        {
            if (requests.ContainsKey(token))
            {
                askUserTasks[token].Wait(TimeSpan.FromSeconds(timeout));
                return requests[token];
            }
            else
            {
                Log.Warn("No access request for token {0}", token);
                return null;
            }
        }

        private String RequestAccessThroughPrivateUSS(string token, string client, string ip, CancellationTokenSource cancelToken)
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
                bool result = channel.RequestAccess(token, client, ip, Configuration.Services.Users.Select(x => x.Username).ToList());
                String selectedUser = null;

                if (result)
                {
                    while (!cancelToken.IsCancellationRequested)
                    {
                        WebAccessRequestResponse response = channel.GetAccessRequestStatus(token);
                        if (response.UserHasResponded)
                        {
                            selectedUser = response.IsAllowed ? response.Username : null;
                            break;
                        }
                        Thread.Sleep(500);
                    }
                }

                if (cancelToken.IsCancellationRequested)
                {
                    channel.CancelAccessRequest(token);
                }
                // close channel
                (channel as ICommunicationObject).Close();
                return selectedUser;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to request access for client {0} at {1} through USS", client, ip), ex);
                return null;
            }
        }

        public bool CancelAccessRequest(string token)
        {
            Log.Info("Cancelling access request with token {0}", token);
            if (cancelTokens.ContainsKey(token))
            {
                cancelTokens[token].Cancel();

                return true;
            }
            else
            {
                Log.Warn("No access request for token {0}", token);
            }
            return false;
        }

        /// <summary>
        /// Cleans up the current access request (remove references,...)
        /// 
        /// Should be called by clients after they have handled the request (read the result or cancelled the request)
        /// </summary>
        /// <param name="token"></param>
        public bool FinishAccessRequest(string token)
        {
            Log.Debug("Removing access request data for token {0}", token);

            if (requests.ContainsKey(token))
            {
                requests.Remove(token);
            }
            else
            {
                Log.Warn("No access request for token {0}", token);
                return false;
            }

            if (cancelTokens.ContainsKey(token))
            {
                cancelTokens.Remove(token);
            }

            if (askUserTasks.ContainsKey(token))
            {
                askUserTasks.Remove(token);
            }

            return true;
        }
    }
}
