#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using Microsoft.Deployment.WindowsInstaller;

namespace MPExtended.Installers.CustomActions
{
    public class CustomAction
    {
        [CustomAction]
        public static ActionResult InstallWifiRemote(Session session)
        {
            Log.Session = session;
            Log.Write("WifiRemote: Entered InstallWifiRemote");

            try
            {
                ActionResult ret = WifiRemote.Install(session);
                if(ret == ActionResult.Success)
                {
                    Log.Write("WifiRemote: Installed successfully");
                    return ActionResult.Success;
                }
                else
                {
                    Log.Write("WifiRemote: Failed to install; result {0}", ret);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Exception during installation: {0}", ex.Message);
            }

            Log.Write("WifiRemote: Leave InstallWifiRemote");
            return ActionResult.Failure;
        }
    }
}
