using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MPExtended.Scrapers.MediaManager.FileManagement
{
    internal class CopyJob
    {
        internal MediaItem Item { get; set; }
        internal FileInfo SourceFile { get; set; }
        internal DirectoryInfo DestinationDirectory { get; set; }
        internal String NewFileName { get; set; }//if set, will rename the sourcefile to NewFileName
        internal bool DeleteSource { get; set; }
        internal bool Backup { get; set; }
    }
}
