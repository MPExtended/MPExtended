using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebResult
    {
        public bool Result { get; set; }

        public WebResult()
        {
        }

        public WebResult(bool value)
        {
            Result = value;
        }

        public static implicit operator WebResult(bool value)
        {
            return new WebResult(value);
        }

        public static implicit operator bool(WebResult value)
        {
            return value.Result;
        }
    }
}
