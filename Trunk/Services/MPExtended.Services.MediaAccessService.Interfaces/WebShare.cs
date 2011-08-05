using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebShare
    {
        public int ShareId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public String[] Extensions { get; set; }
        public String PinCode { get; set; }
        public bool IsFtp { get; set; }
        public String FtpServer { get; set; }
        public int FtpPort { get; set; }
        public String FtpPath { get; set; }
        public String FtpLogin { get; set; }
        public String FtpPassword { get; set; }
    }
}