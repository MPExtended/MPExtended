using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Scrapers.MediaManager.FileManagement
{
    public enum FileCopyProgress { 
        unmoved, 
        queued, 
        existing, 
        copying, 
        aborted, 
        moved, 
        nodest, 
        failed, 
        nospaceleft, 
        doesntexist 
    }
}
