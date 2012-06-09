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
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    internal class MEFControllerFactory : DefaultControllerFactory
    {
        private Dictionary<string, Type> controllerMap = new Dictionary<string, Type>();

        public MEFControllerFactory(HttpContextBase context)
        {
            AggregateCatalog catalog = new AggregateCatalog();

            // load everything from the bin directory for plugins
            foreach (var dir in Plugins.ListPluginDirectories())
            {
                if (Directory.Exists(Path.Combine(dir, "bin")))
                    catalog.Catalogs.Add(new DirectoryCatalog(Path.Combine(dir, "bin")));
            }

            // allow skins to include binaries too (though they shouldn't)
            string currentSkinDirectory = Path.Combine(context.Server.MapPath(SkinnableViewEngine.GetCurrentSkinDirectory(context)), "bin");
            if (Directory.Exists(currentSkinDirectory))
                catalog.Catalogs.Add(new DirectoryCatalog(currentSkinDirectory));

            // do the composition
            CompositionContainer container = new CompositionContainer(catalog);
            var controllers = container.GetExports<IController, IDictionary<string, object>>();

            // and build up a proper map of controller names
            foreach (var export in controllers)
            {
                string fullControllerName = export.Metadata.ContainsKey("controllerName") ?
                    export.Metadata["controllerName"].ToString() + "Controller" :
                    export.Value.GetType().Name;
                controllerMap[fullControllerName] = export.Value.GetType();
            }
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            string fullControllerName = controllerName + "Controller";
            if (controllerMap.ContainsKey(fullControllerName))
            {
                return base.GetControllerInstance(requestContext, controllerMap[fullControllerName]);
            }

            return base.CreateController(requestContext, controllerName);
        }
    }
}