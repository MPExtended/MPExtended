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
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using MPExtended.Libraries.Service;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    [Export(typeof(IPluginData))]
    internal class PluginData : IPluginData
    {
        public Dictionary<string, string> GetConfiguration(string pluginname)
        {
            if (Configuration.Media.PluginConfiguration.ContainsKey(pluginname))
            {
                return Configuration.Media.PluginConfiguration[pluginname].ToDictionary(x => x.Name, x => x.Value);
            }

            return new Dictionary<string, string>();
        }
    }
}
