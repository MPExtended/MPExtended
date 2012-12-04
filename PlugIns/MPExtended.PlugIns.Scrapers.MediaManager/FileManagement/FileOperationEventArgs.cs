using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MPExtended.Scrapers.MediaManager.FileManagement
{
    public class FileOperationEventArgs
    {
        public enum OperationType { Move, Copy, Delete }
  
        public long Progress { get; set; }

        public FileInfo OldFile { get; set; }
        public FileInfo NewFile { get; set; }

        public FileCopyProgress Result { get; set; }
        internal OperationType Operation { get; set; }

        public FileOperationEventArgs(MediaItem item, FileInfo oldFile, FileInfo newFile, FileCopyProgress result, OperationType opType)
        {
            NewFile = newFile;
            OldFile = oldFile;
            Operation = opType;
            Result = result;
        }

        public FileOperationEventArgs(MediaItem item, FileInfo oldFile, FileInfo newFile, FileCopyProgress status, long progress, OperationType opType)
            : this(item, oldFile, newFile, status, opType)
        {
            Progress = progress;
        }



    }
}
