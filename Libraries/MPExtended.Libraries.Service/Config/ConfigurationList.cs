﻿#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using Microsoft.Xml.Serialization.GeneratedAssembly;
using MPExtended.Libraries.Service.Config.Upgrade;

namespace MPExtended.Libraries.Service.Config
{
    internal sealed class ConfigurationList : Dictionary<ConfigurationFile, IConfigurationSerializer>
    {
        public ConfigurationList(MPExtendedProduct product)
            : base()
        {
            this[ConfigurationFile.Authentication] = new ConfigurationSerializer<Authentication, AuthenticationSerializer, AuthenticationUpgrader>(ConfigurationFile.Authentication, "Authentication.xml", "Services.xml");

            if (product == MPExtendedProduct.Service || product == MPExtendedProduct.Configurator)
            {
                this[ConfigurationFile.MediaAccess] = new ConfigurationSerializer<MediaAccess, MediaAccessSerializer, MediaAccessUpgrader>(ConfigurationFile.MediaAccess, "MediaAccess.xml");
                this[ConfigurationFile.Services] = new ConfigurationSerializer<Services, ServicesSerializer, ServicesUpgrader>(ConfigurationFile.Services, "Services.xml");
                this[ConfigurationFile.Streaming] = new ConfigurationSerializer<Streaming, StreamingSerializer, StreamingUpgrader>(ConfigurationFile.Streaming, "Streaming.xml");
                this[ConfigurationFile.StreamingProfiles] = new ProfilesConfigurationSerializer();
                this[ConfigurationFile.Scraper] = new ConfigurationSerializer<Scraper, ScraperSerializer>(ConfigurationFile.Scraper, "Scraper.xml");
            }

            if (product == MPExtendedProduct.WebMediaPortal)
            {
                this[ConfigurationFile.StreamingPlatforms] = new ConfigurationSerializer<StreamingPlatforms, StreamingPlatformsSerializer>(ConfigurationFile.StreamingPlatforms, "StreamingPlatforms.xml");
            }

            if (product == MPExtendedProduct.WebMediaPortal || product == MPExtendedProduct.Configurator)
            {
                this[ConfigurationFile.WebMediaPortalHosting] = new ConfigurationSerializer<WebMediaPortalHosting, WebMediaPortalHostingSerializer, WebMediaPortalHostingUpgrader>(ConfigurationFile.WebMediaPortalHosting, "WebMediaPortalHosting.xml");
                this[ConfigurationFile.WebMediaPortal] = new ConfigurationSerializer<WebMediaPortal, WebMediaPortalSerializer>(ConfigurationFile.WebMediaPortal, "WebMediaPortal.xml");
            }
        }

        public IConfigurationSerializer<TModel> Get<TModel>(ConfigurationFile file) where TModel : class, new()
        {
            return (IConfigurationSerializer<TModel>)this[file];
        }

        public void ForEach(Action<IConfigurationSerializer> action)
        {
            foreach (var serializer in this)
                action(serializer.Value);
        }

        public Dictionary<ConfigurationFile, TResult> ForEach<TResult>(Func<IConfigurationSerializer, TResult> action)
        {
            var result = new Dictionary<ConfigurationFile, TResult>();
            foreach (var serializer in this)
                result[serializer.Key] = action(serializer.Value);
            return result;
        }
    }
}
