using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service.ConfigurationContracts;
using Deusty.Net;
using MPExtended.Libraries.Service.MpConnection.Messages;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace MPExtended.Libraries.Service.MpConnection
{
    public class WifiRemoteClient
    {
        /// <summary>
        /// True if we are logged in via WifiRemote
        /// </summary>
        public bool LoggedIn { get; set; }

        /// <summary>
        /// True if we tried to connect and failed (or connection was closed)
        /// </summary>
        public bool ConnectionFailed { get; set; }

        // TCP connection
        AsyncSocket socket;

        /// <summary>
        /// WifiRemote Address
        /// </summary>
        public String Address { get; set; }

        /// <summary>
        /// WifiRemote port
        /// </summary>
        public int Port { get; set; }

        User authentication;

        /// <summary>
        /// Key used to auto-login
        /// </summary>
        private string autologinKey;

        /// <summary>
        /// The id of the dialog that we sent to MediaPortal
        /// </summary>
        private string currentDialogId;

        /// <summary>
        /// The result of the dialog we sent to MediaPortal
        /// </summary>
        public MessageDialogResult LatestDialogResult;

        public WifiRemoteClient(User authentication, String address, int port)
        {
            this.authentication = authentication;
            this.Address = address;
            this.Port = port;
        }

        /// <summary>
        /// Connect with manually entered info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Connect()
        {
            LoggedIn = false;
            ConnectionFailed = false;
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            Log.Info("WifiRemote connecting to server");

            socket = new AsyncSocket();
            socket.WillConnect += new AsyncSocket.SocketWillConnect(socket_WillConnect);
            socket.DidConnect += new AsyncSocket.SocketDidConnect(socket_DidConnect);
            socket.WillClose += new AsyncSocket.SocketWillClose(socket_WillClose);
            socket.DidClose += new AsyncSocket.SocketDidClose(socket_DidClose);
            socket.DidRead += new AsyncSocket.SocketDidRead(socket_DidRead);
            socket.DidWrite += new AsyncSocket.SocketDidWrite(socket_DidWrite);
            socket.SynchronizingObject = new Form();
            socket.AllowMultithreadedCallbacks = true;
            if (!socket.Connect(Address, 8017))
            {
                Log.Warn("WifiRemote: Could not connect to server, AsyncSocket connect failed");
                ConnectionFailed = true;
            }
        }

        public void Disconnect()
        {
            LoggedIn = false;

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }


        #region Socket connection
        void socket_DidWrite(AsyncSocket sender, long tag)
        {

        }

        void socket_DidRead(AsyncSocket sender, byte[] data, long tag)
        {
            String msg = null;

            try
            {
                msg = Encoding.UTF8.GetString(data);
                // Get json object
                JObject message = JObject.Parse(msg);
                string type = (string)message["Type"];
                if (type != null)
                {
                    // {"Type":"welcome","Server_Version":4,"AuthMethod":0}
                    switch (type)
                    {
                        case "welcome":
                            HandleWelcomeMessage(msg);
                            break;

                        // {"Type":"authenticationresponse","Success":true,"ErrorMessage":null}
                        case "authenticationresponse":
                            // handleAuthenticationResponse((bool)message["Success"], (String)message["ErrorMessage"]);
                            handleAuthenticationResponse(msg);
                            break;
                        case "dialogResult":
                            HandleDialogResult(msg);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("WifiRemote Communication Error", e);
            }

            // Continue listening
            sender.Read(AsyncSocket.CRLFData, -1, 0);
        }

        void socket_DidClose(AsyncSocket sender)
        {

        }

        bool socket_WillConnect(AsyncSocket sender, System.Net.Sockets.Socket socket)
        {
            return true;
        }

        void socket_WillClose(AsyncSocket sender, Exception e)
        {

        }

        /// <summary>
        /// We have a socket connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        void socket_DidConnect(AsyncSocket sender, System.Net.IPAddress address, ushort port)
        {
            if (autologinKey == null)
            {
                Log.Info("Authenticating with server", "Connected, waiting for welcome message", "Authenticating ...");
            }
            else
            {
                Log.Info("Reconnected to server", "Reconnected with autologin key", "Connected");
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
        public void SendCommand(BaseMessage message, AsyncSocket client)
        {
            if (message == null)
            {
                Log.Warn("WifiRemote SendMessage failed: IMessage object is null");
                return;
            }

            if (autologinKey != null)
            {
                message.AutologinKey = autologinKey;
            }

            String msgString = JsonConvert.SerializeObject(message);
            SendCommand(msgString, client);
        }

        /// <summary>
        /// Send a message string
        /// </summary>
        /// <param name="message"></param>
        /// <param name="client"></param>
        public void SendCommand(String message, AsyncSocket client)
        {
            if (message == null)
            {
                Log.Error("WifiRemote SendMessage failed: Message string is null");
                return;
            }

            if (client == null)
            {
                Log.Error("SendMessage aborted: Not connected", "Please connect first!");
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(message + "\r\n");
            client.Write(data, -1, 0);
        }

        private void HandleDialogResult(string msg)
        {
            MessageDialogResult dialogResult = (MessageDialogResult)JsonConvert.DeserializeObject(msg, typeof(MessageDialogResult));
            if (dialogResult.DialogId == currentDialogId)
            {
                LatestDialogResult = dialogResult;
            }
        }

        private void HandleWelcomeMessage(string msg)
        {
            MessageWelcome welcomeMsg = (MessageWelcome)JsonConvert.DeserializeObject(msg, typeof(MessageWelcome));

            // We have some autologin going, ignore welcome message
            if (autologinKey != null) return;

            switch (welcomeMsg.AuthMethod)
            {
                // AuthMethod: User&Password or Both
                case 1:
                case 3:
                    MessageIdentify identificationMessage = new MessageIdentify();
                    identificationMessage.Authenticate = new Authenticate(authentication.Username, authentication.GetPassword());
                    SendCommand(identificationMessage, socket);
                    break;
                // AuthMethod: Passcode
                case 2:
                    Log.Warn("Passcode authentication not available");
                    break;

                // AuthMethod: None
                case 0:
                default:
                    SendCommand(new MessageIdentify(), socket);
                    break;
            }
        }

        private void handleAuthenticationResponse(string msg)
        {
            MessageAuthenticationResponse authResponse = (MessageAuthenticationResponse)JsonConvert.DeserializeObject(msg, typeof(MessageAuthenticationResponse));
   
            LoggedIn = authResponse.Success;
            autologinKey = authResponse.AutologinKey;

            if (!authResponse.Success)
            {
                Log.Warn("WifiRemote Failed to authenticate with server: " + authResponse.ErrorMessage);
            }
        }

        public void SendShowYesNoDialogRequest(String title, String text)
        {
            MessageShowDialog msg = new MessageShowDialog();
            msg.DialogId = new Random(10000).Next().ToString();
            currentDialogId = msg.DialogId;
            msg.DialogType = "yesno";
            msg.Title = title;
            msg.Text = text;

            SendCommand(msg, socket);
        }

        public void SendRequestAccessDialog(string clientName, string ip, List<String> users)
        {
            //TODO: use translated strings for the dialog
            //string msg = String.Format(UI.AccessRequest, clientName, ip);

            SendShowSelectDialogRequest("GimmeGimme", "All ya base belong to us", users);
        }

        private void SendShowSelectDialogRequest(string title, string text, List<string> listOptions)
        {
            MessageShowDialog msg = new MessageShowDialog();
            msg.DialogId = new Random(10000).Next().ToString();
            currentDialogId = msg.DialogId;
            msg.DialogType = "select";
            msg.Title = title;
            msg.Text = text;
            msg.Options = listOptions;

            SendCommand(msg, socket);
        }

        public void CancelRequestAccessDialog()
        {
            //TODO: Cancel dialog in MediaPortal
        }

        #endregion


    }
}
