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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class SkinHelper
    {
        private static SkinTranslationsFactory _translationsFactory = new SkinTranslationsFactory();

        private ViewContext viewContext;

        public string Name { get; set; }
        public SkinTranslations Translations { get; set; }
        public SkinConfiguration Configuration { get; set; }

        public SkinHelper(ViewContext context)
        {
            viewContext = context;

            Name = Settings.ActiveSettings.Skin;
            Translations = _translationsFactory.Create(viewContext, this);
            Configuration = new SkinConfiguration();
        }


        public string GenerateDownloadToken(WebMediaType mediaType, string itemId)
        {
            string tokenData = String.Format("{0}_{1}_{2}", mediaType, itemId, viewContext.HttpContext.Application["randomToken"]);
            byte[] tokenBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(tokenData));
            return BitConverter.ToString(tokenBytes).Replace("-", "").ToLower();
        }

        public object GetDownloadArguments(WebMediaType mediaType, string itemId)
        {
            return new
            {
                item = itemId,
                type = mediaType,
                token = GenerateDownloadToken(mediaType, itemId)
            };
        }
    }
}