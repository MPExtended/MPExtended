using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.Common.Interfaces
{
    public class WebFilterOperator
    {
        public String Operator { get; set; }
        public String Title { get; set; }
        public List<String> SuitableTypes { get; set; }
    }
}
