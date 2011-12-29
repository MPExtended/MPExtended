#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class MetaService : IMetaService
    {
        private const int API_VERSION = 4;

        public WebBool TestConnection()
        {
            return true;
        }

        public WebServiceVersion GetVersion()
        {
            return new WebServiceVersion()
            {
                Version = VersionUtil.GetVersionName(),
                Build = VersionUtil.GetBuildVersion().ToString(),
                ApiVersion = API_VERSION,
            };
        }

        public IList<WebService> GetInstalledServices()
        {
            return Installation.GetInstalledServices()
                .Where(x => x.ServiceName != MPExtendedService.WifiRemote)
                .Select(x => x.ToWebService())
                .ToList();
        }

        public IList<WebService> GetActiveServices()
        {
            return GetInstalledServices();
        }

        public IList<WebServiceSet> GetActiveServiceSets()
        {
            return ServiceDetector.GetSetComposer().ComposeUnique().ToList();
        }

        public WebBool HasUI()
        {
            return Installation.IsServiceInstalled(MPExtendedService.UserSessionService);
        }
    }
}
