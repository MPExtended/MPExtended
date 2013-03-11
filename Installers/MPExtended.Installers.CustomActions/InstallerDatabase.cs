#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
    internal static class InstallerDatabase
    {
        public static bool ExtractBinaryToFile(Session session, string name, string path)
        {
            try
            {
                string sql = "SELECT `Binary`.`Data` FROM `Binary` WHERE `Name` = '" + name + "'";
                using (View binView = session.Database.OpenView(sql))
                {
                    binView.Execute();
                    using (Record binRecord = binView.Fetch())
                    {
                        binRecord.GetStream(1, path);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                session.Log("Extracting binary data {0} to file {1} failed: {2}", name, path, ex.Message);
                return false;
            }
        }

        public static string GetProductProperty(Session session, string name)
        {
            try
            {
                string sql = "SELECT `Property`.`Value` FROM `Property` WHERE `Property`.`Property` = '" + name + "'";
                using (View binView = session.Database.OpenView(sql))
                {
                    binView.Execute();
                    using (Record binRecord = binView.Fetch())
                    {
                        return binRecord.GetString(1);
                    }
                }
            }
            catch (Exception ex)
            {
                session.Log("Loading product property {0} failed: {1}", name, ex.Message);
                return null;
            }
        }
    }
}
