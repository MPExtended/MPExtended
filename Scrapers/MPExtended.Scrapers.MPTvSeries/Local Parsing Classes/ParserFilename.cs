#region GNU license
// MP-TVSeries - Plugin for Mediaportal
// http://www.team-mediaportal.com
// Copyright (C) 2006-2007
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion


using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WindowPlugins.GUITVSeries
{
    public class FilenameParser
    {
        private string m_Filename = string.Empty;
        private string m_FileNameAfterReplacement = string.Empty;
        private Dictionary<string, string> m_Matches = new Dictionary<string, string>();
        private List<string> m_Tags = new List<string>();
        private String m_RegexpMatched = string.Empty;
        private int m_RegexpMatchedIdx = 0;
        static List<String> sExpressions = new List<String>();
        static List<Regex> regularExpressions = new List<Regex>();
        static Dictionary<Regex, string> replacementRegexBefore = new Dictionary<Regex, string>();
        static Dictionary<Regex, string> replacementRegexAfter = new Dictionary<Regex, string>();
        static List<string> tags = new List<string>();

        public Dictionary<string, string> Matches
        {
            get { return m_Matches;}
        }

        public List<string> Tags
        {
            get { return m_Tags; }
        }

        public String RegexpMatched
        {
            get { return m_RegexpMatched; }
        }

        public int RegexpMatchedIndex
        {
            get { return m_RegexpMatchedIdx; }
        }

        public string FileNameAfterReplacement
        {
            get { return m_FileNameAfterReplacement; }
        }

        /// <summary>
        /// Loads and compile Parsing Expressions and also String Replacements
        /// </summary>
        /// <returns></returns>
        public static bool reLoadExpressions()
        {
            // build a list of all the regular expressions to apply
            bool error = false;
            try
            {
                MPTVSeriesLog.Write("Compiling Parsing Expressions");
                sExpressions.Clear();
                regularExpressions.Clear();
                replacementRegexAfter.Clear();
                replacementRegexBefore.Clear();
                DBExpression[] expressions = DBExpression.GetAll();
                foreach (DBExpression expression in expressions)
                {
                    if (expression[DBExpression.cEnabled] != 0)
                    {
                        String sExpression = String.Empty;
                        switch ((String)expression[DBExpression.cType])
                        {
                            case DBExpression.cType_Simple:
                                sExpression = ConvertSimpleExpressionToRegEx(expression[DBExpression.cExpression]);
                                break;

                            case DBExpression.cType_Regexp:
                                sExpression = expression[DBExpression.cExpression];
                                break;
                        }
                        sExpression = sExpression.ToLower();
                        // replace series, season and episode by the valid DBEpisode column names
                        sExpression = sExpression.Replace("<series>", "<" + DBSeries.cParsedName + ">");
                        sExpression = sExpression.Replace("<season>", "<" + DBEpisode.cSeasonIndex + ">");
                        sExpression = sExpression.Replace("<episode>", "<" + DBEpisode.cEpisodeIndex + ">");
                        sExpression = sExpression.Replace("<episode2>", "<" + DBEpisode.cEpisodeIndex2 + ">");
                        sExpression = sExpression.Replace("<title>", "<" + DBEpisode.cEpisodeName + ">");
                        sExpression = sExpression.Replace("<firstaired>", "<" + DBOnlineEpisode.cFirstAired + ">");

                        // we precompile the expressions here which is faster in the end
                        try
                        {
                            regularExpressions.Add(new Regex(sExpression, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled));
                            sExpressions.Add(sExpression);
                        }
                        catch (Exception e)
                        {
                            // wrong regex
                            MPTVSeriesLog.Write("Cannot use the following Expression: " + e.Message);
                        }
                    }
                }
                MPTVSeriesLog.Write("Finished Compiling Parsing Expressions, found " + sExpressions.Count.ToString() + " valid expressions");
            }
            catch (Exception ex)
            {
                MPTVSeriesLog.Write("Error loading Parsing Expressions: " + ex.Message);
                error = true;
            }
                // now go for the replacements
            try
            {
                MPTVSeriesLog.Write("Compiling Replacement Expressions");
                
                foreach (DBReplacements replacement in DBReplacements.GetAll())
                {
                    try
                    {
                        if (replacement[DBReplacements.cEnabled])
                        {
                            String searchString = replacement[DBReplacements.cToReplace];
                            searchString = searchString
                                .Replace("<space>", " ");
                            string regexSearchString = searchString;
                            if (!replacement[DBReplacements.cIsRegex])
                                regexSearchString = Regex.Escape(searchString);

                            String replaceString = replacement[DBReplacements.cWith];
                            replaceString = replaceString
                                .Replace("<space>", " ")
                                .Replace("<empty>", "");

                            var replaceRegex = new Regex(regexSearchString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            
                            if (replacement[DBReplacements.cBefore])
                                replacementRegexBefore.Add(replaceRegex, replaceString);
                            else
                                replacementRegexAfter.Add(replaceRegex, replaceString);

                            if (replacement[DBReplacements.cTagEnabled])
                                tags.Add(searchString);
                        }
                    }
                    catch (Exception e)
                    {
                        MPTVSeriesLog.Write("Cannot use the following Expression: " + e.Message);
                    }
                }
                MPTVSeriesLog.Write("Finished Compiling Replacement Expressions, found " + (replacementRegexBefore.Count + replacementRegexAfter.Count).ToString() + " valid expressions");
                
                return error;
            }
            catch (Exception ex)
            {
                MPTVSeriesLog.Write("Error loading String Replacements: " + ex.Message);
                return false;
            }
        }

        string RunReplacements(Dictionary<Regex, string> replacements, string runAgainst)
        {
            foreach (var replacement in replacements)
            {
              if (replacement.Key.IsMatch(runAgainst)
                    && tags.Contains(replacement.Key.ToString()))
              {
                m_Tags.Add(replacement.Key.ToString());
              }

              runAgainst = replacement.Value.Equals("<RomanToArabic>", StringComparison.OrdinalIgnoreCase) // special case romans
                         ? replacement.Key.Replace(runAgainst, m => " " + Parse1To19RomanNumberOrKeep(m.Value) + " ") 
                         : replacement.Key.Replace(runAgainst, replacement.Value);
            }
          return runAgainst;
        }

        public FilenameParser(string filename)
        {
            try
            {
                ////////////////////////////////////////////////////////////////////////////////////////////
                // Parsing filename for all recognized naming formats to extract episode information
                ////////////////////////////////////////////////////////////////////////////////////////////
                m_Filename = filename;
                if (sExpressions.Count == 0) reLoadExpressions();

                int index = 0;
               
                // run Before replacements
                m_FileNameAfterReplacement = RunReplacements(replacementRegexBefore, m_Filename);
                
                foreach(Regex regularExpression in regularExpressions)
                {
                    Match matchResults;
                    matchResults = regularExpression.Match(m_FileNameAfterReplacement);

                    if (matchResults.Success)
                    {
                        for (int i = 1; i < matchResults.Groups.Count; i++)
                        {
                            string GroupName = regularExpression.GroupNameFromNumber(i);
                            string GroupValue = matchResults.Groups[i].Value;

                            if (GroupValue.Length > 0 && GroupName != "unknown")
                            {
                                // ´run after replacements on captures
                                GroupValue = RunReplacements(replacementRegexAfter, GroupValue);

                                GroupValue = GroupValue.Trim();
                                m_Matches.Add(GroupName, GroupValue);
                            }
                        }
                        // stop on the first successful match
                        m_RegexpMatched = sExpressions[index];
                        m_RegexpMatchedIdx = index;
                        return;
                    }
                  index++;
                }
            }
            catch (Exception ex)
            {
                MPTVSeriesLog.Write("And error occured in the 'FilenameParser' function (" + ex.ToString() + ")");
            }
        }

        private static string ConvertSimpleExpressionToRegEx(string SimpleExpression)
        {
            string field = "";
            string finalRegEx = "";
            int openTagLocation = -1;
            int closeTagLocation = 0;

            SimpleExpression = SimpleExpression.Replace(@"\", @"\\");
            SimpleExpression = SimpleExpression.Replace(".", @"\.");


            while (true)
            {
                openTagLocation = SimpleExpression.IndexOf('<', closeTagLocation);

                if (openTagLocation == -1)
                {
                    if (closeTagLocation > 0)
                        finalRegEx += SimpleExpression.Substring(closeTagLocation + 1);
                    else
                        finalRegEx += SimpleExpression;

                    break;
                }

                if (closeTagLocation == 0)
                    finalRegEx = SimpleExpression.Substring(0, openTagLocation);
                else
                    finalRegEx += SimpleExpression.Substring(closeTagLocation + 1, openTagLocation - closeTagLocation - 1);

                closeTagLocation = SimpleExpression.IndexOf('>', openTagLocation);

                field = SimpleExpression.Substring(openTagLocation + 1, closeTagLocation - openTagLocation - 1);

                if (field.Length > 0)
                {
                    // other tags coming? put lazy *, otherwise put a greedy one
                    if (SimpleExpression.IndexOf('<', closeTagLocation) != -1)
                        finalRegEx += String.Format(@"(?<{0}>[^\\]*?)", field);
                    else
                        finalRegEx += String.Format(@"(?<{0}>[^\\]*)", field);
                }
                else
                {
                    // other tags coming? put lazy *, otherwise put a greedy one
                    if (SimpleExpression.IndexOf('<', closeTagLocation) != -1)
                        finalRegEx += @"(?:[^\\]*?)";
                    else
                        finalRegEx += @"(?:[^\\]*)";
                }
            }

            return finalRegEx;
        }

        static string Parse1To19RomanNumberOrKeep(string roman)
        {
          var intRepresentation = Parse1To19RomanNumber(roman);
          return intRepresentation.HasValue ? intRepresentation.ToString() : roman;
        }

        static int? Parse1To19RomanNumber(string roman)
        {
          roman = roman.ToUpper().Trim();
          switch (roman)
          {
            case "I":
              return 1;
            case "II":
              return 2;
            case "III":
              return 3;
            case "IV":
              return 4;
            case "V":
              return 5;
            case "VI":
              return 6;
            case "VII":
              return 7;
            case "VIII":
              return 8;
            case "IX":
              return 9;
            case "X":
              return 10;
            case "XI":
              return 11;
            case "XII":
              return 12;
            case "XIII":
              return 13;
            case "XIV":
              return 14;
            case "XV":
              return 15;
            case "XVI":
              return 16;
            case "XVII":
              return 17;
            case "XVIII":
              return 18;
            case "XIX":
              return 19;
            default:
              return null;
          }
        }
    }
}
