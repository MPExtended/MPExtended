using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.Common.Interfaces
{
    public class WebFilter
    {
        public String Field { get; set; }
        public String Operator { get; set; }
        public String Value { get; set; }
        public String Conjunction { get; set; }
    }
}
