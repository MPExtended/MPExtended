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
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace MPExtended.Libraries.Service.Util
{
    public static class RegistryReader
    {
        public static object ReadKey(RegistryHive hive, RegistryView view, string subkey, string name)
        {
            // open root
            RegistryKey rootKey = Microsoft.Win32.RegistryKey.OpenBaseKey(hive, view);
            if (rootKey == null)
            {
                return null;
            }

            // look for key
            RegistryKey key = rootKey.OpenSubKey(subkey);
            if (key == null)
            {
                return null;
            }

            // look for value
            object value = key.GetValue(name);
            if (value == null)
            {
                return null;
            }

            return value;
        }

        public static object ReadKey(RegistryHive hive, string subkey, string name)
        {
            return ReadKey(hive, RegistryView.Default, subkey, name);
        }

        public static object ReadKeyAllViews(RegistryHive hive, string subkey, string name)
        {
            object value = ReadKey(hive, RegistryView.Registry64, subkey, name);
            if (value == null)
            {
                value = ReadKey(hive, RegistryView.Registry32, subkey, name);
            }

            return value;
        }
    }
}
