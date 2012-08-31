using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.Common.Interfaces
{
    public class WebFilterOperator
    {
        public string Operator { get; set; }
        public string Title { get; set; }
        public List<string> SuitableTypes { get; set; }
    }
}
