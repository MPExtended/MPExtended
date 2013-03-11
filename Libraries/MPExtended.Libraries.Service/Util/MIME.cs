#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Runtime.InteropServices;

namespace MPExtended.Libraries.Service.Util
{
    public static class MIME
    {
        [DllImport("urlmon.dll", CharSet = CharSet.Auto)]
        private extern static uint FindMimeFromData(
            uint pBC,
            [MarshalAs(UnmanagedType.LPStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            uint cbSize,
            [MarshalAs(UnmanagedType.LPStr)] string pwzMimeProposed,
            uint dwMimeFlags,
            out uint ppwzMimeOut,
            uint dwReserved
        );

        public static string GetFromFilename(string filename, string defaultValue)
        {
            object mime = RegistryReader.ReadKey(Microsoft.Win32.RegistryHive.ClassesRoot, Path.GetExtension(filename), "Content Type");
            return mime == null ? defaultValue : mime.ToString();
        }

        public static string GetFromFilename(string filename)
        {
            return GetFromFilename(filename, null);
        }

        public static string GetFromContent(Stream content, string defaultValue)
        {
            byte[] buffer = new byte[256];
            content.Read(buffer, 0, 256);

            try
            {
                uint mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mimeType = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                return mimeType;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string GetFromContent(Stream content)
        {
            return GetFromContent(content, null);
        }

        public static string GetFromContent(string filename, string defaultValue)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                return GetFromContent(stream, defaultValue);
            }
        }

        public static string GetFromContent(string filename)
        {
            return GetFromContent(filename, null);
        }
    }
}
