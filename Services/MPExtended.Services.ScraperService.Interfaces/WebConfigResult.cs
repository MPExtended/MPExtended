using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    public class WebConfigResult
    {
        public string PluginAssemblyName { get; set; }
        public String DllPath { get; set; }
        public IList<String> ExternalPaths { get; set; }
    }
}
