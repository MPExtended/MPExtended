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
using System.ServiceModel;
using System.Text;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class WCFMetaService : IMetaService, IProtectedMetaService
    {
        public WebBoolResult TestConnection()
        {
            return MetaService.Instance.TestConnection();
        }

        public IList<WebService> GetInstalledServices()
        {
            return MetaService.Instance.GetInstalledServices();
        }

        public IList<WebService> GetActiveServices()
        {
            return MetaService.Instance.GetActiveServices();
        }

        public IList<WebServiceSet> GetActiveServiceSets()
        {
            return MetaService.Instance.GetActiveServiceSets();
        }

        public WebBoolResult HasUI()
        {
            return MetaService.Instance.HasUI();
        }

        public WebServiceVersion GetVersion()
        {
            return MetaService.Instance.GetVersion();
        }

        public void DummyMethod()
        {
            return;
        }
    }
}
