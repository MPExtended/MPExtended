#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Shared
{
    public class ChannelLogos
    {
        private IEnumerable<FileInfo> fileInformation;

        public IEnumerable<string> GetSearchDirectories()
        {
            string cacheDir = GetCacheDirectory();
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            List<string> dirs = new List<string>() 
            {
                Configuration.Streaming.TVLogoDirectory,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal", "thumbs", "tv", "logos"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal", "thumbs", "tv", "logo"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Team MediaPortal", "MediaPortal", "thumbs", "Radio"),
                cacheDir
            };

            if (Mediaportal.GetLocation(MediaportalDirectory.Skin) != null)
            {
                dirs.Add(Path.Combine(Mediaportal.GetLocation(MediaportalDirectory.Skin), "aMPed", "Media", "Logos", "Channels"));
            }

            return dirs
                .Where(dir => Directory.Exists(dir))
                .Distinct();
        }

        public string FindLocation(string channelName)
        {
            if(fileInformation == null)
            {
                fileInformation = GetSearchDirectories()
                    .Select(dir => new DirectoryInfo(dir))
                    .SelectMany(dir => dir.GetFiles());
            }

            return fileInformation
                .Where(fi => Path.GetFileNameWithoutExtension(fi.Name).ToLowerInvariant() == PathUtil.StripInvalidCharacters(channelName, '_').ToLowerInvariant())
                .Select(fi => fi.FullName)
                .FirstOrDefault();
        }

        public void WriteToCacheDirectory(string channelName, string logoFormat, Stream logo)
        {
            string fileName = PathUtil.StripInvalidCharacters(channelName, '_');
            string path = Path.Combine(GetCacheDirectory(), String.Format("{0}.{1}", fileName, logoFormat));
            if (!File.Exists(path))
            {
                using (FileStream writeStream = File.Open(path, FileMode.CreateNew, FileAccess.Write))
                {
                    logo.CopyTo(writeStream);
                }
            }
        }

        private string GetCacheDirectory()
        {
            return Path.Combine(Installation.GetCacheDirectory(), "TVLogoCache");
        }
    }
}
