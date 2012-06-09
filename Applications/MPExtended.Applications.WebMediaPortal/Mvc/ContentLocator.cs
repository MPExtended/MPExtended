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
using System.Web;
using MPExtended.Applications.WebMediaPortal.Code;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class ContentLocator
    {
        public static ContentLocator Current { get; internal set; } 

        // Class implementation
        private HttpServerUtility serverUtility;
        private string currentSkin;

        public List<string> ViewDirectories { get; private set; }
        public List<string> ContentDirectories { get; private set; }

        public ContentLocator(HttpServerUtility server, string activeSkin)
        {
            this.serverUtility = server;
            this.currentSkin = activeSkin;
            Setup();
        }

        public ContentLocator(HttpServerUtility server)
            : this(server, Settings.ActiveSettings.Skin)
        {
        }

        public void ChangeSkin(string skin)
        {
            this.currentSkin = skin;
            Setup();
        }

        private void Setup()
        {
            ViewDirectories = new List<string>();
            ContentDirectories = new List<string>();

            if (currentSkin != null && Directory.Exists(serverUtility.MapPath(String.Format("~/Skins/{0}", currentSkin))))
                ViewDirectories.Add(String.Format("~/Skins/{0}", currentSkin));
            if (currentSkin != null && Directory.Exists(serverUtility.MapPath(String.Format("~/Skins/{0}/Content", currentSkin))))
                ContentDirectories.Add(String.Format("~/Skins/{0}/Content", currentSkin));

            foreach (var plugin in Plugins.ListPlugins())
            {
                if (Directory.Exists(serverUtility.MapPath(String.Format("~/Plugins/{0}/Views", plugin))))
                    ViewDirectories.Add(String.Format("~/Plugins/{0}/Views", plugin));
                if (Directory.Exists(serverUtility.MapPath(String.Format("~/Plugins/{0}/Content", plugin))))
                    ContentDirectories.Add(String.Format("~/Plugins/{0}/Content", plugin));
            }

            ViewDirectories.Add("~/Views");
            ContentDirectories.Add("~/Content");
        }

        public string LocateContent(string fileName)
        {
            foreach (var dir in ContentDirectories)
            {
                string path = dir + "/" + fileName;
                if (File.Exists(serverUtility.MapPath(path)))
                    return path;
            }

            throw new FileNotFoundException(String.Format("Cannot find the content file {0}", fileName));
        }

        public string LocateView(string fileName)
        {
            foreach (var dir in ViewDirectories)
            {
                string path = dir + "/" + fileName;
                if (File.Exists(serverUtility.MapPath(path)))
                    return path;
            }

            throw new FileNotFoundException(String.Format("Cannot find the view file {0}", fileName));
        }
    }
}