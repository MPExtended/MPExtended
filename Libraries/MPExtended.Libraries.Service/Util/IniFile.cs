#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Text;
using System.Text.RegularExpressions;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Service.Util
{
    public class IniFile
    {
        private static readonly Regex sectionRegex = new Regex(@"^\s*\[(\w+)\]\s*$", RegexOptions.Compiled);
        private static readonly Regex lineRegex = new Regex(@"^\s*([^=]+?)\s*=\s*(.*?)\s*$", RegexOptions.Compiled);

        private Dictionary<string, Dictionary<string, string>> sections;

        public IniFile(string path)
        {
            sections = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = null;
            Match match;

            foreach (var line in File.ReadLines(path))
            {
                if (String.IsNullOrWhiteSpace(line))
                    continue;

                match = sectionRegex.Match(line);
                if (match.Success)
                {
                    currentSection = match.Groups[1].Value;
                    sections[currentSection] = new Dictionary<string, string>();
                    continue;
                }

                match = lineRegex.Match(line);
                if (match.Success)
                {
                    sections[currentSection].Add(match.Groups[1].Value, match.Groups[2].Value);
                    continue;
                }

                throw new InvalidDataException(String.Format("Failed to parse line '{0}' in INI file", line));
            }
        }

        public Dictionary<string, Dictionary<string, string>> GetSections()
        {
            return sections;
        }

        public Dictionary<string, string> GetSection(string section)
        {
            if(sections.ContainsKey(section))
                return sections[section];
            throw new KeyNotFoundException(String.Format("Section '{0}' not found", section));
        }

        public Dictionary<string, string> this[string section]
        {
            get
            {
                return GetSection(section);
            }
        }

        public string GetValue(string section, string key)
        {
            var data = GetSection(section);
            if (data.ContainsKey(key))
                return data[key];
            throw new KeyNotFoundException(String.Format("Key '{0}' not found in section '{1}'", key, section));
        }
    }
}
