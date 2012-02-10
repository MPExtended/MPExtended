using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    public class WebServiceSet
    {
        public string MAS { get; set; }
        public string MASStream { get; set; }

        public string TAS { get; set; }
        public string TASStream { get; set; }

        public string UI { get; set; }

        public bool IsSubsetOf(WebServiceSet compare)
        {
            return
                !IsSameAs(compare) &&
                (this.MAS == null || this.MAS == compare.MAS) &&
                (this.MASStream == null || this.MASStream == compare.MASStream) &&
                (this.TAS == null || this.TAS == compare.TAS) &&
                (this.TASStream == null || this.TASStream == compare.TASStream) &&
                (this.UI == null || this.UI == compare.UI);
        }

        public bool IsSameAs(WebServiceSet compare)
        {
            return
                this.MAS == compare.MAS &&
                this.MASStream == compare.MASStream &&
                this.TAS == compare.TAS &&
                this.TASStream == compare.TASStream &&
                this.UI == compare.UI;
        }
    }
}
