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
using MPExtended.Libraries.Service;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal static class WebServiceSetExtensionMethods
    {
        public static bool IsSubsetOf(this WebServiceSet we, WebServiceSet compare)
        {
            return 
                !IsSameAs(we, compare) && 
                (we.MAS == null || we.MAS == compare.MAS) &&
                (we.MASStream == null || we.MASStream == compare.MASStream) &&
                (we.TAS == null || we.TAS == compare.TAS) &&
                (we.TASStream == null || we.TASStream == compare.TASStream) &&
                (we.UI == null || we.UI == compare.UI);
        }

        public static bool IsSameAs(this WebServiceSet we, WebServiceSet compare)
        {
            return
                we.MAS == compare.MAS &&
                we.MASStream == compare.MASStream &&
                we.TAS == compare.TAS &&
                we.TASStream == compare.TASStream &&
                we.UI == compare.UI;
        }
    }

    internal static class ServiceConfigurationExtensionMethods
    {
        public static WebService ToWebService(this ServiceConfiguration service)
        {
            switch(service.Service)
            {
                case MPExtendedService.MediaAccessService:
                    return WebService.MediaAccessService;
                case MPExtendedService.StreamingService:
                    return WebService.StreamingService;
                case MPExtendedService.TVAccessService:
                    return WebService.TVAccessService;
                case MPExtendedService.UserSessionService:
                    return WebService.UserSessionService;
                case MPExtendedService.MetaService:
                    return WebService.MetaService;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
