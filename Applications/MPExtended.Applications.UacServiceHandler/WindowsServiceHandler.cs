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
using System.Reflection;
using System.ServiceProcess;

namespace MPExtended.Applications.UacServiceHandler
{
    internal enum ServiceCommand
    {
        Start,
        Stop,
        Restart
    }

    internal class WindowsServiceHandler
    {
        private string serviceName;

        public WindowsServiceHandler(string serviceName)
        {
            this.serviceName = serviceName;
        }

        public void Execute(string command)
        {
            Dictionary<string, ServiceCommand> map = new Dictionary<string, ServiceCommand>()
            {
                { "start", ServiceCommand.Start },
                { "stop", ServiceCommand.Stop },
                { "restart", ServiceCommand.Restart }
            };

            if (!map.ContainsKey(command))
            {
                throw new ArgumentException("Invalid command", "command");
            }

            Execute(map[command]);
        }

        public void Execute(ServiceCommand command)
        {
            ServiceController sc = new ServiceController(serviceName);
            switch (command)
            {
                case ServiceCommand.Start:
                    sc.Start();
                    break;
                case ServiceCommand.Stop:
                    sc.Stop();
                    break;
                case ServiceCommand.Restart:
                    RestartService(sc, 20000);
                    break;
                default:
                    throw new ArgumentException("Invalid command", "command");
            }
        }

        private void RestartService(ServiceController sc, int timeoutMilliseconds)
        {
            // The timeout is the total timeout for two operations, so we reduce the timeout with the elapsed time after the first operation.
            int startTime = Environment.TickCount;
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }

            timeout -= TimeSpan.FromTicks(Environment.TickCount - startTime);
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }
    }
}
