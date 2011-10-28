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
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return true;
            }

            return base.CanConvert(type);
        }

        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (String.IsNullOrWhiteSpace(parameter))
                {
                    return null;
                }

                parameterType = parameterType.GetGenericArguments().First();
            }

            return base.ConvertStringToValue(parameter, parameterType);
        }

        public override string ConvertValueToString(object parameter, Type parameterType)
        {
            if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (parameter == null)
                {
                    return "";
                }

                parameterType = parameterType.GetGenericArguments().First();
            }

            return base.ConvertValueToString(parameter, parameterType);
        }
    }
}
