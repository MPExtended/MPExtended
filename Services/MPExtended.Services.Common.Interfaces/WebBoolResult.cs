﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.Common.Interfaces
{
    public class WebBoolResult
    {
        public bool Result { get; set; }

        public WebBoolResult()
        {
        }

        public WebBoolResult(bool value)
        {
            Result = value;
        }

        public override string ToString()
        {
            return Result.ToString();
        }

        public override bool Equals(object obj)
        {
            WebBoolResult r = obj is bool ? new WebBoolResult((bool)obj) : obj as WebBoolResult;
            return (object)r != null && this.Result == r.Result;
        }

        public override int GetHashCode()
        {
            return Result ? 1 : 0;
        }

        public static bool operator ==(WebBoolResult a, WebBoolResult b)
        {
            return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Result == b.Result);
        }

        public static bool operator !=(WebBoolResult a, WebBoolResult b)
        {
            return !(a == b);
        }

        public static implicit operator WebBoolResult(bool value)
        {
            return new WebBoolResult(value);
        }

        public static implicit operator bool(WebBoolResult value)
        {
            return value.Result;
        }
    }
}
