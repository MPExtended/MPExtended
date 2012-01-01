#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
//
// The use and distribution terms for this software are covered by the
// Common Public License 1.0 (http://opensource.org/licenses/cpl1.0.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by
// the terms of this license.
//    
// You must not remove this notice, or any other, from this software.
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
