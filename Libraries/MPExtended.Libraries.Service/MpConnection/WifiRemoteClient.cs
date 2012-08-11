#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
// Copyright (C) 2011 - 2012, WifiRemote Developers, http://code.google.com/p/wifiremote/
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
using System.Runtime.Serialization.Json;
using System.Net;
using System.Text;
using Deusty.Net;
using MPExtended.Libraries.Service.ConfigurationContracts;
using MPExtended.Libraries.Service.MpConnection.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MPExtended.Libraries.Service.MpConnection
{
    public class WifiRemoteClient
    {
        /// <summary>
        /// WifiRemote Address
        /// </summary>
        public String Address { get; set; }

        /// <summary>
        /// WifiRemote port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Authentication information we send to WifiRemote
        /// </summary>
        public User AuthenticationInformation { get; set; }

        public bool Connected { get; private set; }
        public bool ConnectionFailed { get; private set; }
        public bool Authenticated { get; private set; }
        public bool AuthenticationFailed { get; private set; }
        public int ServerVersion { get; private set; }

        /// <summary>
        /// The result of the dialog we sent to MediaPortal
        /// </summary>
        public MessageDialogResult LatestDialogResult { get; protected set; }

        /// <summary>
        /// The latest dialog message we received from MediaPortal
        /// </summary>
        public MessageDialog CurrentDialog { get; protected set; }

        // TCP connection
        private AsyncSocket socket;

        /// <summary>
        /// Key used to auto-login
        /// </summary>
        private string autologinKey;

        /// <summary>
        /// The id of the dialog that we sent to MediaPortal
        /// </summary>
        protected string currentDialogId;

        public WifiRemoteClient(User authentication, String address, int port)
        {
            this.AuthenticationInformation = authentication;
            this.Address = address;
            this.Port = port;
        }

        /// <summary>
        /// Connect with manually entered info
        /// </summary>
        public void Connect()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            Connected = false;
            ConnectionFailed = false;
            Authenticated = false;
            AuthenticationFailed = false;

            Log.Info("Setting up WifiRemote connection to {0}:{1}", Address, Port);

            socket = new AsyncSocket();
            socket.DidConnect += new AsyncSocket.SocketDidConnect(socket_DidConnect);
            socket.DidRead += new AsyncSocket.SocketDidRead(socket_DidRead);
            socket.AllowMultithreadedCallbacks = true;
            if (!socket.Connect(Address, 8017))
            {
                Log.Warn("WifiRemoteClient: Could not connect to server, AsyncSocket connect failed");
                ConnectionFailed = true;
            }
        }

        public void Disconnect()
        {
            Connected = false;
            Authenticated = false;

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        #region Socket connection
        private void socket_DidRead(AsyncSocket sender, byte[] data, long tag)
        {
            try
            {
                string msg = Encoding.UTF8.GetString(data);
                // Get json object
                JObject message = JObject.Parse(msg);
                string type = (string)message["Type"];
                if (type != null)
                {
                    switch (type)
                    {                    
                        // {"Type":"welcome","Server_Version":4,"AuthMethod":0}
                        case "welcome":
                            HandleWelcomeMessage(msg);
                            break;

                        // {"Type":"authenticationresponse","Success":true,"ErrorMessage":null}
                        case "authenticationresponse":
                            HandleAuthenticationResponse(msg);
                            break;
                        case "dialogResult":
                            HandleDialogResult(msg);
                            break;
                        case "dialog":
                            HandleDialogMessage(msg);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("WifiRemoteClient: Communication error", e);
            }

            // Continue listening
            sender.Read(AsyncSocket.CRLFData, -1, 0);
        }

        /// <summary>
        /// We have a socket connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        private void socket_DidConnect(AsyncSocket sender, IPAddress address, ushort port)
        {
            Connected = true;
            ConnectionFailed = false;

            if (autologinKey == null)
            {
                Log.Debug("WifiRemoteClient: Authenticating with server, waiting for welcome message...");
            }
            else
            {
                Log.Debug("WifiRemoteClient: Reconnected to server with autologin key");
            }

            socket.Read(AsyncSocket.CRLFData, -1, 0);
        }
        #endregion

        #region Messages and Commands
        /// <summary>
        /// Send a message object
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void SendCommand(BaseMessage message)
        {
            if (message == null)
            {
                Log.Warn("WifiRemoteClient.SendCommand failed: message is null");
                return;
            }

            if (autologinKey != null)
            {
                message.AutologinKey = autologinKey;
            }

            String msgString = JsonConvert.SerializeObject(message);
            SendCommand(msgString);
        }

        /// <summary>
        /// Send a message string
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void SendCommand(String message)
        {
            if (message == null)
            {
                Log.Warn("WifiRemoteClient.SendCommand failed: message string is null");
                return;
            }

            if (!Connected)
            {
                Log.Error("WifiRemoteClient.SendCommand: not connected, connect first!");
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(message + "\r\n");
            socket.Write(data, -1, 0);
        }

        private void HandleDialogResult(string msg)
        {
            MessageDialogResult dialogResult = (MessageDialogResult)JsonConvert.DeserializeObject(msg, typeof(MessageDialogResult));
            if (dialogResult.DialogId == currentDialogId)
            {
                LatestDialogResult = dialogResult;
            }
        }

        private void HandleDialogMessage(string msg)
        {
            MessageDialog dialogResult = (MessageDialog)JsonConvert.DeserializeObject(msg, typeof(MessageDialog));
            CurrentDialog = dialogResult;
        }

        private void HandleWelcomeMessage(string msg)
        {
            MessageWelcome welcomeMsg = (MessageWelcome)JsonConvert.DeserializeObject(msg, typeof(MessageWelcome));
            ServerVersion = welcomeMsg.Server_Version;

            // We have some autologin going, ignore welcome message
            if (autologinKey != null) return;

            switch (welcomeMsg.AuthMethod)
            {
                // AuthMethod: User&Password or Both
                case 1:
                case 3:
                    MessageIdentify identificationMessage = new MessageIdentify();
                    identificationMessage.Authenticate = new Authenticate(AuthenticationInformation.Username, AuthenticationInformation.GetPassword());
                    SendCommand(identificationMessage);
                    break;

                // AuthMethod: Passcode
                case 2:
                    Log.Warn("WifiRemoteClient: Passcode authentication not yet implemented");
                    break;

                // AuthMethod: None
                case 0:
                default:
                    SendCommand(new MessageIdentify());
                    break;
            }
        }

        private void HandleAuthenticationResponse(string msg)
        {
            MessageAuthenticationResponse authResponse = (MessageAuthenticationResponse)JsonConvert.DeserializeObject(msg, typeof(MessageAuthenticationResponse));

            Authenticated = authResponse.Success;
            autologinKey = authResponse.AutologinKey;

            if (!authResponse.Success)
            {
                AuthenticationFailed = true;
                Log.Warn("WifiRemoteClient: Failed to authenticate: '{0}'", authResponse.ErrorMessage);
            }
        }
        #endregion
    }
}
