#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using MPExtended.Libraries.General;

namespace MPExtended.Libraries.Social
{
    internal class JSONUtil
    {
        public static string ToJSON(object obj)
        {
            if (obj == null)
            {
                return String.Empty;
            }

            using (var ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(obj.GetType());
                ser.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T FromJSON<T>(string json)
        {
            if (String.IsNullOrEmpty(json))
            {
                return default(T);
            }

            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json.ToCharArray())))
                {
                    var ser = new DataContractJsonSerializer(typeof(T));
                    return (T)ser.ReadObject(ms);
                }
            }
            catch (Exception ex)
            {
                Log.Info("Failed to parse JSON", ex);
                return default(T);
            }
        }
    }
}
