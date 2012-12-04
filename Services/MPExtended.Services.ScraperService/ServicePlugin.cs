using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MPExtended.Libraries.Service.Hosting;

namespace MPExtended.Services.ScraperService
{
    [Export(typeof(IWcfService))]
    [ExportMetadata("ServiceName", "ScraperService")]
    internal class ServicePlugin : IWcfService
    {
        public void Start()
        {
            // We don't need initialization
        }

        public Type GetServiceType()
        {
            return typeof(ScraperService);
        }
    }
}
