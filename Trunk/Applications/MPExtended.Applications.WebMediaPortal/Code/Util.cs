using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMediaPortal.Code
{
    public static class Util
    {
        public static string FormatDataString(string data)
        {
            string[] items = data.Split('|');
            return String.Join(" - ", items.Skip(1).Take(items.Count() - 2));
        }
    }
}