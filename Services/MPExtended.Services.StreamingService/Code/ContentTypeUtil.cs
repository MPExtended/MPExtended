#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace MPExtended.Services.StreamingService.Code
{
    // There really isn't a better way to do this?!
    internal static class ContentTypeUtil
    {
        public static string GetContentTypeFromExtension(string extension)
        {
            RegistryKey rk = Registry.ClassesRoot.OpenSubKey(extension);
            if (rk != null && rk.GetValue("Content Type") != null)
            {
                return rk.GetValue("Content Type").ToString();
            }

            return null;
        }
    }
}
