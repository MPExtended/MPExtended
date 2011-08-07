#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.ServiceModel;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.ServiceHosts.Console.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Debug("MPExtended.ServiceHosts.Console.Server starting...");

            ServiceHost host1 = new ServiceHost(typeof(MPExtended.Services.TVAccessService.TVAccessService));
            ServiceHost host2 = new ServiceHost(typeof(MPExtended.Services.StreamingService.StreamingService));
            Log.Debug("Opening ServiceHost...");

            host1.Open();
            host2.Open();
            Log.Debug("Host opened");

            Log.Info("MPExtended.ServiceHosts.Console.Server started...");
            NLog.LogManager.Flush();

            System.Console.WriteLine("Press ENTER to close");
            System.Console.ReadLine();

            host1.Close();
            host2.Close();

            Log.Info("MPExtended.ServiceHosts.Console.Server closed...");
        }
    }
}
