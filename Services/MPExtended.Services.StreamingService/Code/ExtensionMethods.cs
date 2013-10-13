#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Security.Cryptography;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Transcoders;

namespace MPExtended.Services.StreamingService.Code
{
    internal static class ResolutionExtensionMethods
    {
        public static WebResolution ToWebResolution(this Resolution res)
        {
            return new WebResolution()
            {
                Width = res.Width,
                Height = res.Height
            };
        }
    }

    internal static class TranscoderProfileExtensionMethods
    {
        public static WebTranscoderProfile ToWebTranscoderProfile(this TranscoderProfile profile)
        {
            // WCF sucks a bit with returning child classes
            return new WebTranscoderProfile()
            {
                Bandwidth = profile.Bandwidth,
                Description = profile.Description,
                HasVideoStream = profile.HasVideoStream,
                MaxOutputHeight = profile.MaxOutputHeight,
                MaxOutputWidth = profile.MaxOutputWidth,
                MIME = profile.MIME,
                Name = profile.Name,
                Targets = profile.Targets,
                Transport = profile.Transport
            };
        }
    }

    internal static class MediaSourceExtensionMethods
    {
        /// <summary>
        /// Copyright (C) Moving-Pictures, http://code.google.com/p/moving-pictures/
        /// 
        /// Taken from MoPi Code: FileInfoExtensions.cs
        /// 
        /// Calculates a unique hash for the contents of the file.
        /// Use this method to compute hashes of large files.
        /// </summary>
        /// <param name="self"></param>
        /// <returns>a unique hash or null when error</returns>
        public static string ComputeSmartHash(this MediaSource self)
        {
            string hexHash = null;
            byte[] bytes = null;
            try
            {
                using (Stream input = self.Retrieve())
                {
                    ulong lhash;
                    long streamsize;
                    streamsize = input.Length;
                    lhash = (ulong)streamsize;

                    long i = 0;
                    byte[] buffer = new byte[sizeof(long)];
                    input.Position = 0;
                    while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
                    {
                        i++;
                        unchecked { lhash += BitConverter.ToUInt64(buffer, 0); }
                    }

                    input.Position = Math.Max(0, streamsize - 65536);
                    i = 0;
                    while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
                    {
                        i++;
                        unchecked { lhash += BitConverter.ToUInt64(buffer, 0); }
                    }
                    bytes = BitConverter.GetBytes(lhash);
                    Array.Reverse(bytes);

                    // convert to hexadecimal string
                    hexHash = bytes.ToHexString();
                }
            }
            catch (Exception e)
            {
                Log.Warn("Error computing smart hash: ", e);
            }
            return hexHash;
        }

        public static string ComputeFullHash(this MediaSource self)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = self.Retrieve())
                {
                    return md5.ComputeHash(stream).ToHexString();
                }
            }
        }
    }
}