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
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MPExtended.Libraries.Client;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public static class Validators
    {
        public static ValidationResult ValidateTASUrl(object data)
        {
            var holder = new MPEServicesHolder(null, (string)data);
            return holder.HasTASConnection ? ValidationResult.Success : new ValidationResult("Failed to access service");
        }

        public static ValidationResult ValidateMASUrl(object data)
        {
            var holder = new MPEServicesHolder((string)data, null);
            return holder.HasMASConnection ? ValidationResult.Success : new ValidationResult("Failed to access service");
        }
    }
}