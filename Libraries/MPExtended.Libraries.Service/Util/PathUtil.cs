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
using System.IO;

namespace MPExtended.Libraries.Service.Util
{
    public static class PathUtil
    {
        public static string StripInvalidCharacters(string path)
        {
            return StripInvalidCharacters(path, Path.GetInvalidFileNameChars(), "");
        }

        public static string StripInvalidCharacters(string path, string replacement)
        {
            return StripInvalidCharacters(path, Path.GetInvalidFileNameChars(), replacement);
        }

        public static string StripInvalidCharacters(string path, char replacement)
        {
            return StripInvalidCharacters(path, Path.GetInvalidFileNameChars(), replacement);
        }

        public static string StripInvalidCharacters(string path, char[] invalid, string replacement)
        {
            foreach (char ch in invalid)
            {
                path = path.Replace(ch.ToString(), replacement);
            }
            return path;
        }

        public static string StripInvalidCharacters(string path, char[] invalid, char replacement)
        {
            foreach (char ch in invalid)
            {
                path = path.Replace(ch, replacement);
            }
            return path;
        }

        public static string StripFileProtocolPrefix(string path)
        {
            if (path == null)
                return null;

            return path.StartsWith("file://") ? path.Substring(7) : path;
        }

        public static string GetAbsolutePath(string basePath, string relativePath)
        {
            return Path.GetFullPath(Path.Combine(basePath, relativePath));
        }
    }
}
