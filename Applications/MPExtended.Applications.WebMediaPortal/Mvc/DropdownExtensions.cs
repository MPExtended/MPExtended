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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Reflection;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public static class DropdownExtensions
    {
        public static string GetDescription<TEnum>(this TEnum value)
        {
            string name = Enum.GetName(value.GetType(), value);
            FieldInfo info = value.GetType().GetField(name);
            DescriptionAttribute[] attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes.First().Description;
            }
            else
            {
                return name;
            }
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        {
            return EnumDropDownListFor(htmlHelper, expression, null);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            IEnumerable<SelectListItem> items =
                from value in Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                select new SelectListItem()
                {
                    Text = value.GetDescription(),
                    Value = value.ToString()
                };

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }

        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return DropDownListFor(htmlHelper, expression, null);
        }

        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            if(expression.NodeType != ExpressionType.Lambda || expression.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new ArgumentException("Cannot determine dropdown list items - unparsable expression");
            }

            MemberInfo member = (expression.Body as MemberExpression).Member;
            var attributes = (ListChoiceAttribute[])member.GetCustomAttributes(typeof(ListChoiceAttribute), true);
            if (attributes == null || attributes.Length == 0)
            {
                throw new ArgumentException("Cannot determine dropdown list items - no ListChoiceAttribute");
            }

            string propertyName = attributes.First().ListPropertyName;
            PropertyInfo property = htmlHelper.ViewData.Model.GetType().GetProperty(propertyName);
            object value = property.GetValue(htmlHelper.ViewData.Model, null);
            if (!value.GetType().GetInterfaces().Any(x => x == typeof(IEnumerable<SelectListItem>)))
            {
                throw new ArgumentException("Cannot determine dropdown list items - invalid list property specified");
            }

            // do not call on htmlHelper to avoid a stack overflow: apparantly (IEnumerable<SelectListItem>value) is seen as object and we're called again
            return SelectExtensions.DropDownListFor(htmlHelper, expression, (IEnumerable<SelectListItem>)value, htmlAttributes);
        }
    }
}