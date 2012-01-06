#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ListChoiceAttribute : ValidationAttribute
    {
        public string ListPropertyName { get; private set; }
        public bool AllowNull { get; set; }

        public ListChoiceAttribute(string propertyName)
        {
            ListPropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null && AllowNull) 
                return ValidationResult.Success;

            PropertyInfo property = validationContext.ObjectInstance.GetType().GetProperty(ListPropertyName);
            if (property == null)
            {
                return new ValidationResult("Invalid list property specified");
            }

            object propertyValue = property.GetValue(validationContext.ObjectInstance, null);
            if (!propertyValue.GetType().GetInterfaces().Any(x => x == typeof(IEnumerable<SelectListItem>)))
            {
                return new ValidationResult("Invalid list property specified");
            }

            foreach (SelectListItem item in (IEnumerable<SelectListItem>)propertyValue)
            {
                if (item.Value == value.ToString())
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}