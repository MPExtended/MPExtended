using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MPExtended.Scrapers.MediaManager
{
    public class MediaItem
    {
        public FileInfo File { get; set; }

        public virtual String GetDestination()
        {
            return null;
        }
    }
}
