﻿#region Copyright (C) 2011-2020 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2011-2020 MPExtended Developers, http://www.mpextended.com/
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.Runtime.Serialization.Json;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Service.WCF
{
    internal class CustomQueryStringConverter : QueryStringConverter
    {
        public override bool CanConvert(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
                return true;

            if (type == typeof(DateTime))
                return true;

            return base.CanConvert(type);
        }

        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            var underlyingType = Nullable.GetUnderlyingType(parameterType);
            if (underlyingType != null)
            {
                if (String.IsNullOrWhiteSpace(parameter))
                    return null;

                parameterType = underlyingType;
            }

            if (parameterType == typeof(DateTime))
            {
                DateTime retval;
                if (DateTime.TryParse(parameter, out retval))
                {
                    return retval;
                }
                else
                {
                    if (parameter.StartsWith("/Date(") && parameter.EndsWith(")/"))
                    {
                        string input = parameter.Substring(6, parameter.Length - 8);
                        int offset = input.IndexOf("+") != -1 ? input.IndexOf("+") : input.IndexOf("-");
                        string msecs = offset == -1 ? input : input.Substring(0, offset);
                        string tz = offset == -1 ? "0000" : input.Substring(offset + 1, input.Length - offset - 1);
                        double h = 0;
                        double m = 0;
                        DateTime date = new DateTime(1970, 1, 1, 0, 0, 0);
                        try
                        {
                            if (!string.IsNullOrEmpty(tz) && tz.Length == 4)
                            {
                              h = Double.Parse(tz.Substring(0,2)) * 3600000;
                              m = Double.Parse(tz.Substring(2,2)) * 60000;
                              h = (input.IndexOf("+") != -1 ? 1 : -1) * h;
                            }
                            return date.AddMilliseconds(Double.Parse(msecs)).AddMilliseconds(h).AddMilliseconds(m);
                        }
                        catch (Exception ex)
                        {
                            Log.Info(String.Format("Failed to parse datetime {0}", parameter), ex);
                            return null;
                        }
                    }
                }
            }

            return base.ConvertStringToValue(parameter, parameterType);
        }

        public override string ConvertValueToString(object parameter, Type parameterType)
        {
            var underlyingType = Nullable.GetUnderlyingType(parameterType);
            if (underlyingType != null)
            {
                if (parameter == null)
                    return String.Empty;

                parameterType = underlyingType;
            }

            return base.ConvertValueToString(parameter, parameterType);
        }
    }
}
