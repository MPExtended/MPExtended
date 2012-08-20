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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using MPExtended.Applications.WebMediaPortal.Code.Composition;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Applications.WebMediaPortal.Mvc;
using MPExtended.Libraries.Service;
using Config = MPExtended.Libraries.Service.Config;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class Settings
    {
        public static Config.WebMediaPortal ActiveSettings
        {
            get
            {
                return Configuration.WebMediaPortal;
            }
        }

        public static void ApplySkinSettings()
        {
            // Setup everything that uses settings from the current skin
            Log.Debug("Active skin: {0}", ActiveSettings.Skin);
            ContentLocator.Current.ChangeSkin(ActiveSettings.Skin);
            ControllerBuilder.Current.SetControllerFactory(new ControllerFactory());
            foreach (var engine in ViewEngines.Engines.OfType<SkinnableViewEngine>())
            {
                engine.UpdateActiveSkin();
            }
        }
    }
}