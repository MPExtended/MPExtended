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
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MetaService.Interfaces;
using MPExtended.Services.UserSessionService.Interfaces;
using MPExtended.Libraries.Service.MpConnection;
using System.Threading;

namespace MPExtended.Services.MetaService
{
    internal class AccessRequests
    {
        private const string ERROR = "<error>";
        private const int REQUIRED_WIFIREMOTE = 12;

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

            if (!Configuration.Services.AccessRequestEnabled)
            {
                Log.Info("User access request from {0} (claims to be client {1}) denied because user access requests are disabled on this system (check configuration)", ip, clientName);
                request.UserHasResponded = true;
                request.Token = null;
                return request;
            }

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
                    Log.Debug("Requesting user response through WifiRemote");
                    result = RequestAccessThroughWifiRemote(clientName, ip, cancelToken);
                }
                else
                {
                    Log.Debug("Requesting user response through USS");
                    result = RequestAccessThroughPrivateUSS(token, clientName, ip, cancelToken);
                }
                Log.Debug("Finish asking user about access request with token {0}: {1}", token, result);

                // make sure that the request is still active (not cancelled)
                if (requests.ContainsKey(token))
                {
                    lock (requests[token])
                    {
                        // set the necessary flags
                        var matchingUsers = Configuration.Services.Users.Where(x => x.Username == result);
                        requests[token].ErrorDuringProcessing = result == ERROR || !matchingUsers.Any();
                        requests[token].IsAllowed = !requests[token].ErrorDuringProcessing && result != null;
                        requests[token].UserHasResponded = true;
                        if (matchingUsers.Any())
                        {
                            Log.Info("Sending account {0} in response to access request {1}", matchingUsers.First().Username, token);
                            requests[token].Username = matchingUsers.First().Username;
                            requests[token].Password = matchingUsers.First().GetPassword();
                        }
                        else if (result == ERROR)
                        {
                            Log.Error("Failure during access request for token {0}", token);
                        }
                        else if (result != null)
                        {
                            Log.Warn("Didn't find a user named '{0}' - something strange is going on!", result);
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
            var client = new AccessRequestingClient(auth, "localhost", WifiRemote.Port);
            client.Connect();

            bool sentDialog = false;
            while (!client.ConnectionFailed)
            {
                if (client.AuthenticationFailed)
                {
                    Log.Error("Failed to authorize with WifiRemote");
                    break;
                }

                if (client.Authenticated && client.ServerVersion < REQUIRED_WIFIREMOTE)
                {
                    Log.Error("Connected to WifiRemote API {0}, but API {1} is required. Please update your WifiRemote.", client.ServerVersion, REQUIRED_WIFIREMOTE);
                    break;
                }

                if (cancelToken.IsCancellationRequested)
                {
                    if (sentDialog)
                        client.CancelRequestAccessDialog();
                    break;
                }

                if (!sentDialog && client.Authenticated)
                {
                    client.SendRequestAccessDialog(clientName, ip, Configuration.Services.Users.Select(x => x.Username).ToList());
                    sentDialog = true;
                }

                if (client.LatestDialogResult != null)
                {
                    // User accepted or denied the request
                    client.Disconnect();
                    return client.LatestDialogResult.SelectedOption;
                }

                Thread.Sleep(500);
            }

            client.Disconnect();
            return ERROR;
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
                return ERROR;
            }
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
