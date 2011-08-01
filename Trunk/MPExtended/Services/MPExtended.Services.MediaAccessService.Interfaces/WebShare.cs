using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebShare
    {
        public int ShareId;
        public string Path;
        public string Name;
        public String[] Extensions;
        public String PinCode;
        public bool IsFtp;
        public String FtpServer;
        public int FtpPort;
        public String FtpPath;
        public String FtpLogin;
        public String FtpPassword;

        public WebShare() { }
        public WebShare(int _shareId, string _name, string _path)
        {
            this.ShareId = _shareId;
            this.Path = _path;
            this.Name = _name;
        }

    }
}