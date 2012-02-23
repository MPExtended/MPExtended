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
using System.Threading;
using System.Threading.Tasks;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Services.Common.Interfaces;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    public class MetaService : IMetaService, IProtectedMetaService
    {
        #region Initialization
        private const string STARTUP_CONDITION = "MetaService";

        public static MetaService Instance { get; private set; }

        public static void Setup()
        {
            if (Instance == null)
            {
                Instance = new MetaService();
            }
        }

        private IEnumerable<IServicePublisher> publishers;
        private IServiceDetector detector;
        private bool initialized;
        private AccessRequests accessRequests;

        public MetaService()
        {
            ServiceState.RegisterStartupCondition(STARTUP_CONDITION);

            CompositionHinter hinter = new CompositionHinter(new ZeroconfDiscoverer());
            hinter.StartDiscovery();
            publishers = hinter.GetAvailablePublishers();
            detector = new CachingServiceDetector(new AdhocServiceDetector(hinter), hinter);

            initialized = false;
            ServiceState.Started += delegate()
            {
                initialized = true;
            };

            Task.Factory.StartNew(delegate()
            {
                // give it some time to detect all service sets
                Thread.Sleep(5000);
                foreach (var publisher in publishers)
                {
                    publisher.PublishAsync(detector);
                }
                ServiceState.StartupConditionCompleted(STARTUP_CONDITION);
            });

            accessRequests = new AccessRequests();
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

        public WebBoolResult TestConnection()
        {
            return initialized;
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

        public WebBoolResult HasUI()
        {
            return detector.HasUI;
        }

        public WebAccessRequestResponse CreateAccessRequest(string clientName)
        {
            return accessRequests.CreateAccessRequest(clientName);
        }

        public WebAccessRequestResponse GetAccessRequestStatus(string token)
        {
            return accessRequests.GetAccessRequestStatus(token);
        }

        public WebAccessRequestResponse GetAccessRequestStatusBlocking(string token, int timeout)
        {
            return accessRequests.GetAccessRequestStatusBlocking(token, timeout);
        }
        #endregion

        #region IProtectedMetaService implementation
        public void DummyMethod()
        {
            return;
        }
        #endregion
    }
}
