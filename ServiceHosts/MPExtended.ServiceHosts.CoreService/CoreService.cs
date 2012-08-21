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
using System.Threading;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Hosting;

namespace MPExtended.ServiceHosts.CoreService
{
    public partial class CoreService : ServiceBase
    {
        private MPExtendedHost host;

        public CoreService()
        {
            InitializeComponent();
            this.ServiceName = "MPExtended Service";
        }

        protected override void OnStart(string[] args)
        {
            LogRotation.Rotate();
            Log.Setup("Service.log", false);
            Installation.Load(MPExtendedProduct.Service);
            host = new MPExtendedHost();
            host.Open();
        }

        protected override void OnStop()
        {
            host.Close();
        }
    }
}
