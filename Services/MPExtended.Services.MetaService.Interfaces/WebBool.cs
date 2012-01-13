using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MetaService.Interfaces
{
    public class WebBool
    {
        public bool Value { get; set; }

        public WebBool()
        {
        }

        public WebBool(bool value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            WebBool r = obj is bool ? new WebBool((bool)obj) : obj as WebBool;
            return (object)r != null && this.Value == r.Value;
        }

        public override int GetHashCode()
        {
            return Value ? 1 : 0;
        }

        public static bool operator ==(WebBool a, WebBool b)
        {
            return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Value == b.Value);
        }

        public static bool operator !=(WebBool a, WebBool b)
        {
            return !(a == b);
        }

        public static implicit operator WebBool(bool value)
        {
            return new WebBool(value);
        }

        public static implicit operator bool(WebBool value)
        {
            return value.Value;
        }
    }
}
