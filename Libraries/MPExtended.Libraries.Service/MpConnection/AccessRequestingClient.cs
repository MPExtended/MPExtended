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
using System.Text;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.MpConnection.Messages;

namespace MPExtended.Libraries.Service.MpConnection
{
    public class AccessRequestingClient : WifiRemoteClient
    {
        public AccessRequestingClient(User authentication, String address, int port)
            : base(authentication, address, port)
        {
        }

        public void SendShowYesNoDialogRequest(String title, String text)
        {
            MessageShowDialog msg = new MessageShowDialog();
            msg.DialogId = new Random().Next(0, 10000).ToString();
            currentDialogId = msg.DialogId;
            msg.DialogType = "yesno";
            msg.Title = title;
            msg.Text = text;

            SendCommand(msg);
        }

        private void SendShowSelectDialogRequest(string title, string text, List<string> listOptions)
        {
            MessageShowDialog msg = new MessageShowDialog();
            msg.DialogId = new Random().Next(0, 10000).ToString();
            currentDialogId = msg.DialogId;
            msg.DialogType = "yesnoselect";
            msg.Title = title;
            msg.Text = text;
            msg.Options = listOptions;

            SendCommand(msg);
        }

        public void SendRequestAccessDialog(string clientName, string ip, List<String> users)
        {
            string msg = String.Format(Strings.UI.AccessRequestWifiRemote, clientName, ip);
            SendShowSelectDialogRequest("MPExtended", msg, users);
        }

        public void CancelRequestAccessDialog()
        {
            if (CurrentDialog != null && CurrentDialog.DialogShown)
            {
                MpDialog diag = CurrentDialog.Dialog;
                if (diag.AvailableActions.Contains("cancel"))
                {
                    MessageDialogAction action = new MessageDialogAction() { ActionType = "cancel", DialogId = diag.DialogId, DialogType = diag.DialogType };
                    SendCommand(action);
                }
            }
        }
    }
}
