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
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.General;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    [Export(typeof(IPluginData))]
    public class PluginData : IPluginData
    {
        public Dictionary<string, string> GetConfiguration(string pluginname)
        {
            if (MPExtended.Libraries.General.Configuration.Media.PluginConfiguration.ContainsKey(pluginname))
            {
                return MPExtended.Libraries.General.Configuration.Media.PluginConfiguration[pluginname].ToDictionary(x => x.Name, x => x.Value);
            }

            return new Dictionary<string, string>();
        }

        public ILogger Log
        {
            get
            {
                return new LogProxy();
            }
        }
    }
}
