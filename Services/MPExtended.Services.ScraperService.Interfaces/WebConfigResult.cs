using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
{
    /// <summary>
    /// Information about the config ui of a sraper
    /// </summary>
    public class WebConfigResult
    {
        /// <summary>
        /// Assembly name of the config ui
        /// </summary>
        public string PluginAssemblyName { get; set; }

        /// <summary>
        /// Path to the dlls for the config ui
        /// </summary>
        public String DllPath { get; set; }

        /// <summary>
        /// Paths to external DLLs that have to be loaded for the config ui
        /// </summary>
        public IList<String> ExternalPaths { get; set; }
    }
}
