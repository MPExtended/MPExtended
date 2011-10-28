using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public abstract class WebObject
    {
        // ID of the provider from where this object originates
        public int PID { get; set; }
    }
}
