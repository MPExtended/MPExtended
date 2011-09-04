#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService
{
    [Export(typeof(IPluginData))]
    public class PluginData : IPluginData
    {
        private static Dictionary<string, Dictionary<string, string>> cachedConfig = new Dictionary<string, Dictionary<string, string>>();

        public Dictionary<string, string> Configuration
        {
            get
            {
                return ReadConfig(Assembly.GetCallingAssembly());
            }
        }

        public ILogger Log
        {
            get
            {
                return new LogProxy();
            }
        }

        private Dictionary<string, string> ReadConfig(Assembly forAssembly)
        {
            string name = forAssembly.GetName().Name;
            if (!cachedConfig.ContainsKey(name))
            {
                var config = XElement.Load(MPExtended.Libraries.ServiceLib.Configuration.GetPath("MediaAccess.xml"))
                    .Element("pluginConfiguration")
                    .Elements("plugin")
                    .Where(p => p.Attribute("name").Value == name)
                    .First()
                    .Descendants()
                    .Select(n => new KeyValuePair<string, string>(n.Name.LocalName, (string)n.Value))
                    .ToDictionary(x => x.Key, x => PerformFolderSubstitution(x.Value));
                cachedConfig[name] = config;
            }

            return cachedConfig[name];
        }


        private string PerformFolderSubstitution(string input)
        {
            string cappdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return input.Replace("%ProgramData%", cappdata);
        }
    }
}
