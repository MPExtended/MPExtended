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
using System.Reflection;
using System.Resources;
using System.Text;
using System.IO;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Service.Util
{
    public static class CultureDatabase
    {
        public static IEnumerable<CultureInfo> GetLanguages()
        {
            return CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                .Where(x => x.Parent == CultureInfo.InvariantCulture && x.TwoLetterISOLanguageName.Length == 2 && x.TwoLetterISOLanguageName != "iv")
                .GroupBy(x => x.TwoLetterISOLanguageName, (key, items) => items.First());
        }

        public static CultureInfo GetLanguage(string name)
        {
            return GetLanguages().FirstOrDefault(x => 
                x.DisplayName == name || x.EnglishName == name || x.NativeName == name || x.Name == name || 
                x.TwoLetterISOLanguageName == name || x.ThreeLetterISOLanguageName == name || x.ThreeLetterWindowsLanguageName == name);
        }

        public static IEnumerable<CultureInfo> GetAvailableTranslations(ResourceManager rm)
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (var culture in cultures)
            {
                // This is the source language, which is only returned below in some versions of the .NET framework
                if (culture.Name == "en")
                {
                    yield return culture;
                    continue;
                }

                ResourceSet rs = rm.GetResourceSet(culture, true, false);
                if (rs == null || culture.LCID == CultureInfo.InvariantCulture.LCID || culture.Parent.LCID != CultureInfo.InvariantCulture.LCID)
                    continue;
                yield return culture;
            }
        }

        public static CultureInfo GetTranslationCulture()
        {
            return GetLanguage(Configuration.Services.DefaultLanguage);
        }
    }
}
