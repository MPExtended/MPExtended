#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class SkinTranslations
    {
        private const string SECTION_NAME = "translations";
        private Dictionary<string, string> translations;

        internal SkinTranslations(HttpServerUtilityBase serverUtility, string skin, CultureInfo culture)
        {
            string path = serverUtility.MapPath(String.Format("~/Skins/{0}/config/translations.{1}.ini", skin, culture.TwoLetterISOLanguageName));
            while (!File.Exists(path) && culture.Parent != null && culture.Parent != culture)
            {
                culture = culture.Parent;
                path = serverUtility.MapPath(String.Format("~/Skins/{0}/config/translations.{1}.ini", skin, culture.TwoLetterISOLanguageName));
            }

            if (File.Exists(path))
            {
                var iniFile = new IniFile(path);
                var sections = iniFile.GetSections();
                if (sections.ContainsKey(SECTION_NAME))
                {
                    translations = iniFile.GetSection(SECTION_NAME);
                    return;
                }
                
                Log.Error("Translation file {0} doesn't contain required section {1}", path, SECTION_NAME);
            }

            translations = new Dictionary<string, string>();
        }

        internal SkinTranslations(ViewContext context, SkinHelper skinHelper)
            : this (context.HttpContext.Server, skinHelper.Name, Thread.CurrentThread.CurrentUICulture)
        {
        }

        public string Get(string key)
        {
            if (translations.ContainsKey(key))
                return translations[key];

            Log.Error("Requested translation with key '{0}', which as not found", key);
            return String.Format("[WARNING: Translation key '{0}' does not exist.]", key); // neat or ugly?
        }

        public string this[string key]
        {
            get
            {
                return Get(key);
            }
        }
    }
}