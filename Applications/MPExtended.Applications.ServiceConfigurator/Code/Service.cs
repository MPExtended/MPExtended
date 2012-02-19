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
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Applications.UacServiceHandler;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal static class Service
    {
        public const string SERVICE_NAME = "MPExtended Service";

        public static bool ShouldRestart { get; set; }

        public static void RestartService()
        {
            // restart the service
            if (!UacServiceHelper.IsAdmin())
            {
                Log.Debug("Service: no admin rights, use UacServiceHandler");
                UacServiceHelper.RestartService();
            }
            else if (UacServiceHelper.IsAdmin())
            {
                Log.Debug("Service: have admin rights, restart ourselves");
                var handler = new WindowsServiceHandler(SERVICE_NAME);
                handler.Execute(ServiceCommand.Restart);
            }

            ShouldRestart = false;
        }
    }
}
