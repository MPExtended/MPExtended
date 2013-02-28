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
using System.IO;
using System.Reflection;
using System.Text;
using MPExtended.Libraries.Service.Composition;

namespace MPExtended.Libraries.Service.Hosting
{
    internal class ServiceInstallation
    {
        private static ServiceInstallation _instance;
        public static ServiceInstallation Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ServiceInstallation();
                return _instance;
            }
        }

        private IEnumerable<Plugin<IService>> services;
        private IEnumerable<Plugin<IWcfService>> wcfServices;
        
        public IEnumerable<Plugin<IService>> GetServices()
        {
            if (services == null)
                LoadServices();
            return services;
        }

        public IEnumerable<Plugin<IWcfService>> GetWcfServices()
        {
            if (wcfServices == null)
                LoadServices();
            return wcfServices;
        }

        private void LoadServices()
        {
            var loader = new PluginLoader();
            loader.AddDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), false);
            loader.AddFromTreeMatch(@"Services\MPExtended.Services.*", "MPExtended.Services.*.dll", @"Plugins\Services");
            loader.AddRequiredMetadata("ServiceName");
            services = loader.GetPlugins<IService>();
            wcfServices = loader.GetPlugins<IWcfService>();
        }
    }
}
