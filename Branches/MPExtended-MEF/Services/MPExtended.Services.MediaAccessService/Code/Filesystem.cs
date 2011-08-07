#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
using System.IO;
using System.ServiceModel.Web;
using MPExtended.Libraries.ServiceLib;
using MPExtended.Services.MediaAccessService.Interfaces;

namespace MPExtended.Services.MediaAccessService.Code
{
    public class Filesystem
    {
        public static byte[] GetFile(string path)
        {
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader reader = new BinaryReader(fileStream);
                return reader.ReadBytes((int)fileStream.Length);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to read file", ex);
                return null;
            }
        }

        public static Stream GetFileStream(string path)
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                //context.ContentLength = fi.Length;
                context.Headers.Add(System.Net.HttpResponseHeader.CacheControl, "public");
                context.Headers.Add(System.Net.HttpResponseHeader.ContentLength, fi.Length.ToString());
                context.Headers.Add(System.Net.HttpResponseHeader.ContentType, "application/binary");
                context.StatusCode = System.Net.HttpStatusCode.OK;
                context.LastModified = fi.LastWriteTime;

                FileStream fs = File.OpenRead(path);
                return fs;
            }
            catch (Exception ex)
            {
                Log.Error("Error getting file stream " + path, ex);
                return null;
            }
        }

        public static WebFileInfo GetFileInfo(string path)
        {
            if (!File.Exists(path))
            {
                Log.Warn("Trying to get file info of non-existent file {0}", path);
                return null;
            }
            return new FileInfo(path).ToWebFileInfo();
        }
    }
}