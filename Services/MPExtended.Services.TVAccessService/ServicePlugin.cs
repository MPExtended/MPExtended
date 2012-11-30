#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using MPExtended.Libraries.Service.Hosting;

namespace MPExtended.Services.TVAccessService
{
    [Export(typeof(IWcfService))]
    [ExportMetadata("ServiceName", "TVAccessService")]
    internal class ServicePlugin : ISingleInstanceWcfService
    {
        public void Start()
        {
            LogoDownloader.Setup();
        }

        public Type GetServiceType()
        {
            return typeof(TVAccessService);
        }

        public void SetInstance(object instance)
        {
            TVAccessService.Instance = (TVAccessService)instance;
        }
    }
}
