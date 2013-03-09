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
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    internal class SkinTranslationsFactory
    {
        private Dictionary<string, Dictionary<string, SkinTranslations>> _translations;

        public SkinTranslationsFactory()
        {
            _translations = new Dictionary<string, Dictionary<string, SkinTranslations>>();
        }

        public SkinTranslations Create(ViewContext viewContext, string skin, CultureInfo culture)
        {
            if (_translations.ContainsKey(skin) && _translations[skin].ContainsKey(culture.Name))
                return _translations[skin][culture.Name];

            var skinTranslations = new SkinTranslations(viewContext.HttpContext.Server, skin, culture);
            if (!_translations.ContainsKey(skin))
                _translations[skin] = new Dictionary<string, SkinTranslations>();
            _translations[skin][culture.Name] = skinTranslations;

            return skinTranslations;
        }

        public SkinTranslations Create(ViewContext viewContext, SkinHelper skinHelper)
        {
            return Create(viewContext, skinHelper.Name, Thread.CurrentThread.CurrentUICulture);
        }
    }
}