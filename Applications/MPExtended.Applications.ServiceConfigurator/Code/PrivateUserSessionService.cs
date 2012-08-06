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
using System.Windows;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.UserSessionService.Interfaces;
using MPExtended.Applications.ServiceConfigurator.Pages;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    internal class PrivateUserSessionService : IPrivateUserSessionService
    {
        SelectUserDialog diag = null;
        public WebBoolResult OpenConfigurator()
        {
            Application.Current.MainWindow.Show();
            return true;
        }

        public WebStringResult RequestAccess(string clientName, string ipAddress, List<string> users)
        {
            string msg = String.Format(Strings.UI.AccessRequest, clientName, ipAddress);
            //MessageBoxResult result = MessageBox.Show(msg, "MPExtended",  MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            diag = new SelectUserDialog("MpExtended", msg, users);
            diag.Width = 360;
            diag.Height = 220;
            bool? result = diag.ShowDialog();

            return (result != null && result == true ? diag.SelectedUser : null);
        }

        public WebBoolResult CancelAccessRequest()
        {
            if (diag != null)
            {
                diag.Close();
            }
            return true;
        }
    }
}
