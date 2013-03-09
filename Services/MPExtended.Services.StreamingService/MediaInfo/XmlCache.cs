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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Timers;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Hosting;
using MPExtended.Services.StreamingService.Code;

namespace MPExtended.Services.StreamingService.MediaInfo
{
    internal class XmlCache : IMediaInfoCache
    {
        private DataContractSerializer serializer;
        private Dictionary<string, CachedInfoWrapper> cache;
        private bool isDirty = false;
        private string path;
        private Timer flushTimer;

        public XmlCache()
        {
            serializer = new DataContractSerializer(typeof(Dictionary<string, CachedInfoWrapper>));
            path = Path.Combine(Installation.GetCacheDirectory(), "MediaInfo-v2.xml");
            cache = new Dictionary<string, CachedInfoWrapper>();
            if (File.Exists(path))
            {
                try
                {
                    using (Stream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        cache = (Dictionary<string, CachedInfoWrapper>)serializer.ReadObject(inputStream);
                }
                catch (Exception ex)
                {
                    Log.Warn("Loading MediaInfo cache failed", ex);
                }
            }

            // save every minute and on exit
            flushTimer = new Timer(60 * 1000);
            flushTimer.AutoReset = true;
            flushTimer.Elapsed += (object sender, ElapsedEventArgs e) => SaveToDisk();
            flushTimer.Start();
            ServiceState.Stopping += SaveToDisk;
        }

        public bool HasForSource(MediaSource src)
        {
            return cache.ContainsKey(src.GetUniqueIdentifier());
        }

        public CachedInfoWrapper GetForSource(MediaSource src)
        {
            return cache[src.GetUniqueIdentifier()];
        }

        public void Save(MediaSource src, CachedInfoWrapper info)
        {
            lock (cache)
                cache[src.GetUniqueIdentifier()] = info;

            isDirty = true;
        }

        private void SaveToDisk()
        {
            if(!isDirty)
                return;

            // This happens during uninstallation: the Cache directory is already removed, but the service isn't stopped yet. To avoid
            // a harmless but ugly error in the logs, don't try to write the cache when the directory has already been removed.
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Log.Debug("Not writing MediaInfo cache, because cache directory doesn't exist");
                return;
            }

            try
            {
                lock (cache)
                {
                    Log.Debug("Writing {0} items to MediaInfo cache", cache.Count);
                    using (Stream outputStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                        serializer.WriteObject(outputStream, cache);
                }

                isDirty = false;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to serialize MediaInfo to file", ex);
            }
        }
    }
}
