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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Hosting;

namespace MPExtended.ServiceHosts.WebMediaPortal
{
    public partial class HostService : ServiceBase
    {
        private IISExpressHost host;

        public HostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogRotation.Rotate();
            Log.Filename = "WebMediaPortalHosting.log";
            Log.ConsoleLogging = false;
            Log.TraceLogging = false;
            Log.Setup();

            Installation.Load(MPExtendedProduct.WebMediaPortal);
            host = new IISExpressHost();
            Action start = host.Start;
            start.BeginInvoke(delegate(IAsyncResult param)
            {
                start.EndInvoke(param);
            }, host);
        }

        protected override void OnStop()
        {
            host.Stop();
        }
    }
}
