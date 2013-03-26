using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    [XmlRoot(Namespace = "http://mpextended.github.com/schema/config/Scraper/1")]
    public class Scraper
    {
        public List<int> AutoStart { get; set; }
    }
}
