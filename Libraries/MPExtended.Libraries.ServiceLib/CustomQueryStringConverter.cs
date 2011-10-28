#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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

namespace MPExtended.Libraries.ServiceLib
{
    internal class CustomQueryStringConverter : QueryStringConverter
    {
        public override bool CanConvert(Type type)
        {
            if (type == typeof(int?))
            {
                return true;
            }

            return base.CanConvert(type);
        }

        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            if(parameterType == typeof(int?)) 
            {
                int outval;
                if (!String.IsNullOrWhiteSpace(parameter) && Int32.TryParse(parameter, out outval))
                {
                    return outval;
                }
                else
                {
                    return null;
                }
            }

            return base.ConvertStringToValue(parameter, parameterType);
        }

        public override string ConvertValueToString(object parameter, Type parameterType)
        {
            if (parameterType == typeof(int?))
            {
                int? value = (int?)parameter;
                return value.HasValue ? value.Value.ToString() : "";
            }

            return base.ConvertValueToString(parameter, parameterType);
        }
    }
}
