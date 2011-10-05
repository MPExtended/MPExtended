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
using MPExtended.Libraries.General;

namespace MPExtended.ServiceHosts.Console.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Log.Debug("MPExtended.ServiceHosts.Console.SingleSeat starting...");
            var hosts = new List<ServiceHost>()
            {
                new ServiceHost(typeof(MPExtended.Services.MediaAccessService.MediaAccessService)),
                new ServiceHost(typeof(MPExtended.Services.TVAccessService.TVAccessService)),
                new ServiceHost(typeof(MPExtended.Services.StreamingService.StreamingService)),
                new ServiceHost(typeof(MPExtended.Services.UserSessionService.UserSessionProxyService))
            };

            Log.Debug("Opening ServiceHost...");
            foreach (var host in hosts)
            {
                host.Open();
            }
            Log.Debug("Host opened");

            Log.Info("MPExtended.ServiceHosts.Console.SingleSeat started...");
            NLog.LogManager.Flush();

            System.Console.WriteLine("Press ENTER to close");
            System.Console.ReadLine();

            foreach (var host in hosts)
            {
                host.Close();
            }
            Log.Info("MPExtended.ServiceHosts.Console.SingleSeat closed...");
        }
    }
}
