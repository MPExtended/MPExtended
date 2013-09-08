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
    /// <summary>
    /// Helper class for external calls to native calls for dynamic dll loading 
    /// </summary>
    public class NativeAssemblyLoader
    {
        /// <summary>
        /// Adds a directory to the process DLL search path.
        /// 
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/hh310513%28v=vs.85%29.aspx for further information
        /// </summary>
        /// <param name="lpPathName">Path to directory</param>
        /// <returns>Did the function succeed</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AddDllDirectory(string lpPathName);

        /// <summary>
        /// Adds a directory to the search path used to locate DLLs for the application. Each time the SetDllDirectory function is called, it replaces the directory specified in the previous SetDllDirectory call. To specify more than one directory, use the AddDllDirectory function and call LoadLibraryEx with LOAD_LIBRARY_SEARCH_USER_DIRS
        /// 
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/ms686203%28v=vs.85%29.aspx for further information
        /// </summary>
        /// <param name="lpPathName">Path to directory</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetDllDirectory(int bufsize, StringBuilder buf);

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause 
        /// other modules to be loaded.
        ///
        /// For additional load options, use the LoadLibraryEx function.
        /// 
        /// See http://msdn.microsoft.com/en-us/library/windows/desktop/ms684175%28v=vs.85%29.aspx for further information
        /// </summary>
        /// <param name="librayName"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string librayName);
    }
}
