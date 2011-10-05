using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.UserSessionService.Interfaces
{
    public class WebResult
    {
        public WebResult()
        {
        }

        public WebResult(bool status)
        {
            Status = status;
        }

        public bool Status { get; set; }
    }
}
