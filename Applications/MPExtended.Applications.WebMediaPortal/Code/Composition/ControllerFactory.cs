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
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Strings;

namespace MPExtended.Applications.WebMediaPortal.Code.Composition
{
    internal class ControllerFactory : DefaultControllerFactory
    {
        private Dictionary<string, Type> controllerMap = new Dictionary<string, Type>();

        public ControllerFactory()
        {
            // Map all stock controllers
            var controllerInterfaceType = typeof(IController);
            var stockControllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => !type.IsAbstract && controllerInterfaceType.IsAssignableFrom(type));
            foreach (var controller in stockControllers)
            {
                controllerMap[controller.Name] = controller;
            }

            // Map all controllers that came from extensions
            var controllers = Composer.Instance.GetActiveControllers();
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

            // Do not call the base class here, because that one searches *all* loaded assemblies for controllers. This
            // results in the controllers from disabled skins being activated, which is something we've been trying hard
            // to avoid. Disadvantage is that we need to map the stock controllers by ourselves now, but that isn't very 
            // complicated. 

            throw new HttpException(404, "No controller could be found for this request.");
        }
    }
}