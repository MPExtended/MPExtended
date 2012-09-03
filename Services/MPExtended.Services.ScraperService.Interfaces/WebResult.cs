using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.ScraperService.Interfaces
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

        public override string ToString()
        {
            return Result.ToString();
        }

        public override bool Equals(object obj)
        {
            WebResult r = obj is bool ? new WebResult((bool)obj) : obj as WebResult;
            return (object)r != null && this.Result == r.Result;
        }

        public override int GetHashCode()
        {
            return Result ? 1 : 0;
        }

        public static bool operator ==(WebResult a, WebResult b)
        {
            return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Result == b.Result);
        }

        public static bool operator !=(WebResult a, WebResult b)
        {
            return !(a == b);
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
