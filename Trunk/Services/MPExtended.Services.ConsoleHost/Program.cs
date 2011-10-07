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
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Reflection;
using MPExtended.Libraries.General;

namespace MPExtended.Services.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Debug("MPExtended.Services.ConsoleHost starting...");

            var hosts = new List<ServiceHost>()
            {
                new ServiceHost(typeof(MPExtended.Services.MediaAccessService.MediaAccessService)),
                new ServiceHost(typeof(MPExtended.Services.TVAccessService.TVAccessService)),
                new ServiceHost(typeof(MPExtended.Services.StreamingService.StreamingService)),
                new ServiceHost(typeof(MPExtended.Services.UserSessionService.UserSessionProxyService)),
            };

            Log.Debug("Opening ServiceHost...");
            foreach (var host in hosts)
            {
                host.Open();
            }
            Log.Debug("Host opened");

            Log.Info("MPExtended.Services.ConsoleHost started...");
            NLog.LogManager.Flush();

            System.Console.WriteLine("Press ENTER to close");
            System.Console.ReadLine();

            foreach (var host in hosts)
            {
                host.Close();
            }
            Log.Info("MPExtended.Services.ConsoleHost closed...");
        }
    }
}
