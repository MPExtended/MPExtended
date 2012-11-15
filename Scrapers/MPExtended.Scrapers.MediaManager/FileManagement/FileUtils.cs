using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Scrapers.MediaManager.FileManagement
{
    class FileUtils
    {
        /// <summary>
        /// Returns a formatted string with good values (KB, GB,...) from the given bytes
        /// </summary>
        /// <param name="_freeSpace"></param>
        /// <returns></returns>
        internal static String FormatBytes(long _bytes)
        {
            if (_bytes < 1024)
            {
                return _bytes + " Bytes";
            }
            _bytes = _bytes / 1024;
            if (_bytes < 1024)
            {
                return _bytes + " KB";
            }
            _bytes = _bytes / 1024;
            if (_bytes < 1024)
            {
                return _bytes + " MB";
            }
            _bytes = _bytes / 1024;
            return _bytes + " GB";

        }
    }
}
