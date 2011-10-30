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
    internal static class BinaryData
    {
        public static bool ExtractToFile(Session session, string name, string path)
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
                session.Log(String.Format("Extracting binary data {0} to file {1} failed", name, path), ex);
                return false;
            }
        }
    }
}
