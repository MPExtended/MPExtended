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
using System.Threading;
using MPExtended.Libraries.Service;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    public class MetaService : IMetaService
    {
        #region Initialization
        public static MetaService Instance { get; private set; }

        public static void Setup()
        {
            if (Instance == null)
            {
                Instance = new MetaService();
            }
        }

        private IServicePublisher[] publishers;
        private ServiceDetector detector;

        public MetaService()
        {
            detector = new ServiceDetector();
            publishers = new IServicePublisher[] { new ZeroconfPublisher() };

            ThreadManager.Start("ServicePublishing", delegate()
            {
                // give it some time to publish
                Thread.Sleep(5000);
                foreach (var publisher in publishers)
                {
                    publisher.PublishAsync(detector);
                }
            });
        }

        ~MetaService()
        {
            foreach (var publisher in publishers)
            {
                publisher.Unpublish();
            }
        }
        #endregion

        #region IMetaService implementation
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
            return detector.GetInstalledServices();
        }

        public IList<WebService> GetActiveServices()
        {
            return detector.GetActiveServices();
        }

        public IList<WebServiceSet> GetActiveServiceSets()
        {
            return detector.CreateSetComposer().ComposeUnique().ToList();
        }

        public WebBool HasUI()
        {
            return detector.HasUI;
        }
        #endregion
    }
}
