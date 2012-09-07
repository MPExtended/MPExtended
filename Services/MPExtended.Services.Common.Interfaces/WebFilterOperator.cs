using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.Common.Interfaces
{
    public class WebFilterOperator
    {
        public WebFilterOperator()
        {
            SuitableTypes = new List<string>();
        }

        public string Operator { get; set; }
        public string Title { get; set; }
        public IList<string> SuitableTypes { get; set; }
    }
}
