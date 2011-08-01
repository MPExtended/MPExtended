using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebFileInfo
    {

        public WebFileInfo()
        {
            //set default DateTime to prevent issues with DateTime.Min
            this.LastAccessTime = new DateTime(1970, 1, 1);
            this.LastWriteTime = new DateTime(1970, 1, 1);
        }

        public WebFileInfo(String _filePath):this()
        {
            FileInfo info = new FileInfo(_filePath);
            fillInformation(info);
        }

        private void fillInformation(FileInfo info)
        {
            if (info != null)
            {
                this.Length = info.Length;
                this.Name = info.Name;
                this.FullName = info.FullName;
                this.LastAccessTime = info.LastAccessTime;
                this.LastWriteTime = info.LastWriteTime;
                this.IsReadOnly = info.IsReadOnly;
                this.DirectoryName = info.DirectoryName;
                this.Exists = info.Exists;
                this.Extension = info.Extension;
            }
        }

        public WebFileInfo(FileInfo _info):this()
        {
            fillInformation(_info);
        }


        public long Length { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime LastAccessTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string DirectoryName { get; set; }
        public bool Exists { get; set; }
        public string Extension { get; set; }
        public bool IsReadOnly { get; set; }

        
    }
}
