#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Libraries.Service;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class SkinConfiguration 
    {
        public string LoadGlobalSetting(string name)
        {
            return Settings.ActiveSettings.SkinConfiguration[Settings.ActiveSettings.Skin + "_" + name];
        }

        public string LoadUserSetting(string name)
        {
            return Settings.ActiveSettings.SkinConfiguration[Settings.ActiveSettings.Skin + "_" + name];
        }

        public void SaveGlobalSetting(string name, string value)
        {
            Settings.ActiveSettings.SkinConfiguration[Settings.ActiveSettings.Skin + "_" + name] = value;
        }

        public void SaveUserSetting(string name, string value)
        {
            Settings.ActiveSettings.SkinConfiguration[Settings.ActiveSettings.Skin + "_" + name] = value;
        }

        private void WriteToConfiguration()
        {
            Configuration.WebMediaPortal.SkinConfiguration = Settings.ActiveSettings.SkinConfiguration;
            Configuration.Save();
        }
    }
}