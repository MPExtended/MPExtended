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
using MPExtended.ServiceHosts.Hosting;

namespace MPExtended.ServiceHosts.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // optimize for speed in debug builds
            var host = new MPExtendedHost(new List<Type>
            {
                typeof(MPExtended.Services.MediaAccessService.MediaAccessService),
                typeof(MPExtended.Services.TVAccessService.TVAccessService),
                typeof(MPExtended.Services.StreamingService.StreamingService),
                typeof(MPExtended.Services.UserSessionService.UserSessionProxyService),
                typeof(MPExtended.Services.MetaService.WCFMetaService)
            });
#else
            var host = new MPExtendedHost();
#endif
            host.Open();

            ExitDetector.Install(delegate()
            {
                host.Close();
            });

            Console.ReadKey();
            host.Close();
        }
    }
}
