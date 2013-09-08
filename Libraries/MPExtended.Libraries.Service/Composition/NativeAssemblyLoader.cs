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
using System.Text;
using System.Runtime.InteropServices;

namespace MPExtended.Libraries.Service.Composition
{
    public static class NativeAssemblyLoader
    {
        // Don't use AddDllDirectory and RemoveDllDirectory here, because they are only available on
        // Windows 8 (they require KB2533623, a non-important update, on earlier Windows versions).

        /// <summary>
        /// Adds a directory to the search path used to locate DLLs for the application. Each time the SetDllDirectory 
        /// function is called, it replaces the directory specified in the previous SetDllDirectory call.
        /// 
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/ms686203%28v=vs.85%29.aspx.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetDllDirectory(int bufsize, StringBuilder buf);

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause 
        /// other modules to be loaded.
        ///
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/ms684175%28v=vs.85%29.aspx.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public static void SetSearchDirectory(string directory)
        {
            SetDllDirectory(directory);
        }

        public static string GetSearchDirectory()
        {
            var txt = new StringBuilder(1024);
            GetDllDirectory(txt.Length, txt);
            return txt.ToString();
        }

        public static void LoadAssembly(string libraryName)
        {
            // TODO: Convert the IntPtr to a handle and return it, if needed
            LoadLibrary(libraryName);
        }
    }
}
