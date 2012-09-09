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
            //client.Headers[HttpRequestHeader.UserAgent] = VersionUtil.GetUserAgent("DevTool"); <<-- This seems to cause a HTTP 401 response
            client.Encoding = Encoding.UTF8;
            client.Credentials = new NetworkCredential(Answers["username"], Answers["password"]);

            var languages = GetLanguageList(client).ToList();
            foreach (var resource in resources)
            {
                foreach (var code in languages)
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

        private class LanguageStatistics
        {
            public int TranslatedStrings { get; set; }
            public int TotalStrings { get; set; }
        }

        private IEnumerable<string> GetLanguageList(WebClient client)
        {
            var languages = new Dictionary<string, LanguageStatistics>();
            foreach (var resource in resources)
            {
                var stats = client.DownloadString(String.Format("https://www.transifex.com/api/2/project/mpextended/resource/{0}/stats/", resource.Key));
                var response = JObject.Parse(stats);
                foreach (var item in response)
                {
                    if (!languages.ContainsKey(item.Key))
                        languages[item.Key] = new LanguageStatistics() { TranslatedStrings = 0, TotalStrings = 0 };
                    languages[item.Key].TranslatedStrings += (int)item.Value["translated_entities"];
                    languages[item.Key].TotalStrings += (int)item.Value["translated_entities"] + (int)item.Value["untranslated_entities"];
                }
            }

            foreach (var lang in languages)
            {
                float percentage = lang.Value.TranslatedStrings * 1f / lang.Value.TotalStrings;
                if (percentage < 0.8f)
                {
                    OutputStream.WriteLine("Skipping language '{0}' because translated count {1}% is below threshold (80%)", lang.Key, Math.Round(percentage * 100));
                }
                else if (lang.Key != "en") // skip the source language
                {
                    yield return lang.Key;
                }
            }
        }
    }
}
