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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class JQueryHelper
    {
        private HtmlHelper htmlHelper;
        private bool datePickerDefaultOptionsAdded = false;
        private Dictionary<string, bool> datePickerAdded = new Dictionary<string, bool>();
        private JQueryDocumentReadyScript documentReadyScript;

        public JQueryHelper(HtmlHelper htmlHelper)
        {
            this.htmlHelper = htmlHelper;
        }

        public string Enable()
        {
            htmlHelper.Assets().AddScript("~/Scripts/jquery-1.4.4.min.js");

            // return empty string so we can just type @Html.jQuery().Enable()
            return String.Empty;
        }

        public string EnableUI()
        {
            Enable();
            htmlHelper.Assets().AddScript("~/Scripts/jquery-ui.min.js");
            htmlHelper.Assets().AddStylesheet("~/Content/Themes/base/jquery-ui.css");

            // return empty string so we can just type @Html.jQuery().EnableUI()
            return String.Empty;
        }

        public string AddDocumentReadyScript(string script)
        {
            if (documentReadyScript == null)
            {
                documentReadyScript = new JQueryDocumentReadyScript();
                htmlHelper.Assets().AddScriptBlock(documentReadyScript);
            }
            documentReadyScript.AddBlock(script);

            // return empty string so we can just type @Html.jQuery().AddDocumentReadyScript()
            return String.Empty;
        }

        public string AddDatePicker(string elementSelector)
        {
            return CreateDateTimePicker(elementSelector, false, true, false);
        }

        public string AddDatePicker(string elementSelector, bool openOnFocus)
        {
            return CreateDateTimePicker(elementSelector, openOnFocus, true, false);
        }

        public string AddDatePicker(string elementSelector, bool openOnFocus, bool hasCalendarIcon)
        {
            return CreateDateTimePicker(elementSelector, openOnFocus, hasCalendarIcon, false);
        }

        public string AddDateTimePicker(string elementSelector)
        {
            return CreateDateTimePicker(elementSelector, false, true, true);
        }

        public string AddDateTimePicker(string elementSelector, bool openOnFocus)
        {
            return CreateDateTimePicker(elementSelector, openOnFocus, true, true);
        }

        public string AddDateTimePicker(string elementSelector, bool openOnFocus, bool hasCalendarIcon)
        {
            return CreateDateTimePicker(elementSelector, openOnFocus, hasCalendarIcon, true);
        }

        private string CreateDateTimePicker(string elementSelector, bool openOnFocus, bool hasCalendarIcon, bool hasTimepicker)
        {
            if (datePickerAdded.ContainsKey(elementSelector) && datePickerAdded[elementSelector] == hasTimepicker)
            {
                return String.Empty;
            }

            if (!datePickerDefaultOptionsAdded)
            {
                AddDefaultDatePickerOptions();
            }

            // options
            Dictionary<string, string> options = new Dictionary<string, string>();
            options.Add("constrainInput", !hasTimepicker && openOnFocus ? "true" : "false");
            if (hasCalendarIcon)
            {
                options.Add("showOn", openOnFocus ? "'both'" : "'button'");
                options.Add("buttonImage", "'" + UrlHelper.GenerateContentUrl("~/Content/Images/calendar.gif", htmlHelper.ViewContext.HttpContext) + "'");
                options.Add("buttonImageOnly", "true");
            }

            // preserve time string when changing date
            if (hasTimepicker)
            {
                AddDocumentReadyScript("$('" + elementSelector + "').change(function () { $(this).data('dpv', $(this).val()); });");
                AddDocumentReadyScript("$('" + elementSelector + "').each(function () { $(this).data('dpv', $(this).val()); });");
                options.Add("onSelect", "function (nv, f) { var ov = $(this).data('dpv'); $(this).val(ov.indexOf(' ') == -1 ? nv : nv + ov.substr(ov.indexOf(' '))); }");
            }

            // enable it
            AddDocumentReadyScript("$('" + elementSelector + "').datepicker(" + CreateJavascriptObject(options) + ");");
            datePickerAdded[elementSelector] = hasTimepicker;
            return String.Empty;
        }

        private void AddDefaultDatePickerOptions()
        {
            // make sure to work on a copy as we change some settings later on
            var dtfi = (DateTimeFormatInfo)System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.Clone();

            // standard text items (TODO: localize)
            Dictionary<string, string> defaultOptions = new Dictionary<string, string>();
            defaultOptions.Add("closeText", "'Close'");
            defaultOptions.Add("prevText", "'Prev'");
            defaultOptions.Add("nextText", "'Next'");

            // set some date options
            defaultOptions.Add("dateFormat", "'" + MapDatePattern(dtfi.ShortDatePattern) + "'");
            defaultOptions.Add("firstDay", ((int)dtfi.FirstDayOfWeek).ToString()); // DayOfWeek.Sunday is zero

            // set all localized names of months and days, with sunday as first day of the week.
            dtfi.FirstDayOfWeek = DayOfWeek.Sunday;
            defaultOptions.Add("monthNames", CreateJavascriptArray(dtfi.MonthNames));
            defaultOptions.Add("monthNamesShort", CreateJavascriptArray(dtfi.AbbreviatedMonthNames));
            defaultOptions.Add("dayNames", CreateJavascriptArray(dtfi.DayNames));
            defaultOptions.Add("dayNamesShort", CreateJavascriptArray(dtfi.AbbreviatedDayNames));
            defaultOptions.Add("dayNamesMin", CreateJavascriptArray(dtfi.ShortestDayNames));

            // add the actual javascript
            AddDocumentReadyScript("$.datepicker.setDefaults(" + CreateJavascriptObject(defaultOptions) + ");");
            datePickerDefaultOptionsAdded = true;
        }

        private string MapDatePattern(string datePattern)
        {
            var list = new Dictionary<string, string>() {
                { "dddd", "DD" },
                { "ddd", "D" },
                { "dd", "dd" },
                { "d", "d" },
                { "MMMM", "MM" },
                { "MMM", "M" },
                { "MM", "mm" },
                { "M", "m" },
                { "yyyy", "yy" },
                { "yy", "y" }
            };

            Random rand = new Random();
            var replaceBack = new Dictionary<string, string>();
            foreach (var item in list)
            {
                string replaceWith = "jquery-" + rand.Next(10000, 99999).ToString();
                datePattern = datePattern.Replace(item.Key, replaceWith);
                replaceBack.Add(replaceWith, item.Value);
            }

            foreach (var item in replaceBack)
            {
                datePattern = datePattern.Replace(item.Key, item.Value);
            }
            return datePattern;
        }

        private string CreateJavascriptArray(IEnumerable<string> items)
        {
            return "[" + String.Join(", ", items.Where(x => !String.IsNullOrEmpty(x)).Select(x => "'" + x + "'")) + "]";
        }

        private string CreateJavascriptObject(Dictionary<string, string> items)
        {
            return "{" + String.Join(", ", items.Select(x => x.Key + ": " + x.Value)) + "}";
        }
    }
}