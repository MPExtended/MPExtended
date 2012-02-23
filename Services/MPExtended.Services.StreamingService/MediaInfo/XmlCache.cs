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
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.MediaInfo
{
    internal class XmlCache : IMediaInfoCache
    {
        private DataContractSerializer serializer;
        private Dictionary<string, WebMediaInfo> cache;
        private bool isDirty = false;
        private string path;

        public XmlCache()
        {
            serializer = new DataContractSerializer(typeof(Dictionary<string, WebMediaInfo>));
            path = Path.Combine(Installation.GetCacheDirectory(), "MediaInfo.xml");
            if (File.Exists(path))
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                Stream inputStream = File.OpenRead(path);
                cache = (Dictionary<string, WebMediaInfo>)serializer.ReadObject(inputStream);
                inputStream.Close();
                timer.Stop();
                Log.Debug("MediaInfo cache loading took {0} ms for {1} items", timer.ElapsedMilliseconds, cache.Count);
            }
            else
            {
                cache = new Dictionary<string, WebMediaInfo>();
            }

            ThreadManager.Start("MICacheSave", delegate()
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(60 * 1000);
                        if (isDirty)
                        {
                            isDirty = false;
                            SaveToDisk();
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    SaveToDisk();
                }
            });
        }

        public bool HasForSource(MediaSource src)
        {
            return cache.ContainsKey(src.GetUniqueIdentifier());
        }

        public WebMediaInfo GetForSource(MediaSource src)
        {
            return cache[src.GetUniqueIdentifier()];
        }

        public void Save(MediaSource src, WebMediaInfo info)
        {
            lock (cache)
            {
                cache[src.GetUniqueIdentifier()] = info;
            }
            isDirty = true;
        }

        private void SaveToDisk()
        {
            lock (cache)
            {
                Log.Debug("Writing {0} items to MediaInfo cache", cache.Count);
                Stream outputStream = File.OpenWrite(path);
                serializer.WriteObject(outputStream, cache);
                outputStream.Close();
            }
        }
    }
}
