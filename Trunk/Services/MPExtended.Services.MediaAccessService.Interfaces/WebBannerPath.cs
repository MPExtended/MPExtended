using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebBannerPath
    {
        public WebBannerPath() { }
        public WebBannerPath(String _name, String _path, String _virtualPath)
        {
            Name = _name;
            Path = _path;
            VirtualPath = _virtualPath;
        }
        public String Name { get; set; }
        public String Path { get; set; }
        public String VirtualPath { get; set; }
    }
}