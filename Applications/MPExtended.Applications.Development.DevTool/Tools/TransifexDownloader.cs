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
using System.Net;
using System.Text;
using MPExtended.Libraries.Service;
using Newtonsoft.Json.Linq;

namespace MPExtended.Applications.Development.DevTool.Tools
{
    internal class TransifexDownloader : IQuestioningDevTool, IDevTool
    {
        public string Name { get { return "Transifex Downloader"; } }
        public TextWriter OutputStream { get; set; }
        public Dictionary<string, string> Answers { get; set; }

        public IEnumerable<Question> Questions
        {
            get
            {
                return new List<Question>() {
                    new Question("username", "Enter Transifex username: "),
                    new Question("password", "Enter Transifex password: ")
                };
            }
        }

        private Dictionary<string, string> resources = new Dictionary<string, string>()
        {
            { "webmediaportal-forms", @"Applications\MPExtended.Applications.WebMediaPortal\Strings\FormStrings.{0}.resx" },
            { "webmediaportal-user-interface", @"Applications\MPExtended.Applications.WebMediaPortal\Strings\UIStrings.{0}.resx" },
            { "configuration-interface", @"Libraries\MPExtended.Libraries.Service\Strings\UI.{0}.resx" },
        };

        public void Run()
        {
            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = !String.IsNullOrEmpty(VersionUtil.GetGitVersion()) ?
                String.Format("MPExtended/{0} (DevTool; commit {1})", VersionUtil.GetVersionName(), VersionUtil.GetGitVersion()) : 
                String.Format("MPExtended/{0} (DevTool)", VersionUtil.GetVersionName());
            client.Encoding = Encoding.UTF8;
            client.Credentials = new NetworkCredential(Answers["username"], Answers["password"]);

            foreach (var resource in resources)
            {
                var details = client.DownloadString(String.Format("https://www.transifex.com/api/2/project/mpextended/resource/{0}/?details", resource.Key));
                var response = JObject.Parse(details);
                foreach (var code in response["available_languages"].Select(x => (string)x["code"]))
                {
                    var resx = client.DownloadString(String.Format("https://www.transifex.com/api/2/project/mpextended/resource/{0}/translation/{1}/?file", resource.Key, code));
                    var path = Path.Combine(Installation.GetSourceRootDirectory(), String.Format(resource.Value, code));
                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        writer.Write(resx);
                    }
                    OutputStream.WriteLine("Got translation of resource '{0}' for language '{1}'", resource.Key, code);
                }
            }
        }
    }
}
