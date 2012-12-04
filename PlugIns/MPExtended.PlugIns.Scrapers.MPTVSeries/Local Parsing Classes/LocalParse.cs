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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace WindowPlugins.GUITVSeries
{
    public class LocalParse
    {
        private BackgroundWorker worker = null;

        public delegate void LocalParseProgressHandler(int nProgress, List<parseResult> results);
        public delegate void LocalParseCompletedHandler(IList<parseResult> results);
        public event LocalParseProgressHandler LocalParseProgress;
        public event LocalParseCompletedHandler LocalParseCompleted;

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;
            List<String> listFolders = new List<string>();
            DBImportPath[] importPathes = DBImportPath.GetAll();
            if (importPathes != null)
            {
                foreach (DBImportPath importPath in importPathes)
                {                    
                    if (importPath[DBImportPath.cEnabled] != 0)
                    {
                        listFolders.Add(importPath[DBImportPath.cPath]);
                    }
                }
            }
            List<PathPair> files = Filelister.GetFiles(listFolders);
            MPTVSeriesLog.Write("Found " + files.Count.ToString() + " supported video files in your import paths");
            e.Result = Parse(files);
        }

        static DBImportPath[] paths = null;

        //public static bool isOnRemovable(string filename)
        //{
        //    if (paths == null) paths = DBImportPath.GetAll();
        //    foreach (DBImportPath path in paths)
        //    {
        //        if (path[DBImportPath.cRemovable] && filename.ToLower().Contains(path[DBImportPath.cPath].ToString().ToLower())) return true;
        //    }
        //    return false;
        //}
        
        //public static bool needToKeepReference(string filename)
        //{
        //    if (paths == null) paths = DBImportPath.GetAll();
        //    foreach (DBImportPath path in paths)
        //    {
        //        if (path[DBImportPath.cKeepReference] && filename.ToLower().Contains(path[DBImportPath.cPath].ToString().ToLower())) return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// Gets the corresponding Import Path that the filename is attached to
        /// </summary>        
        public static string getImportPath(string filename)
        {
            if (paths == null) paths = DBImportPath.GetAll();
            if (paths != null)
            {
                foreach (DBImportPath path in paths)
                {
                    string importPath = path[DBImportPath.cPath];
                    if (filename.ToLower().Contains(importPath.ToString().ToLower()))
                        return importPath;
                }
            }
            return null;
        }       

        public static List<parseResult> Parse(List<PathPair> files)
        {
            return Parse(files, true);
        }
        public static List<parseResult> Parse(List<PathPair> files, bool includeFailed)
        {
            MPTVSeriesLog.Write(string.Format("Starting Local Filename Parsing, processing {0} files", files.Count.ToString()));
            List<parseResult> results = new List<parseResult>();
            parseResult progressReporter;
            int nFailed = 0;
            FilenameParser parser = null;
            ListViewItem item = null;
            paths = null;
            foreach (PathPair file in files)
            {
                parser = new FilenameParser(file.m_sMatch_FileName);
                
                // title case the seriesname
                if (parser.Matches.ContainsKey(DBSeries.cParsedName))
                    parser.Matches[DBSeries.cParsedName] = parser.Matches[DBSeries.cParsedName].ToString().ToTitleCase();
               
                // set volumelabel for drive so can be prompted to insert CD/DVD disk or removable harddrive
                // populate with import path name if can not get volume label
                string volumeLabel = DeviceManager.GetVolumeLabel(file.m_sFull_FileName);
                
                if (string.IsNullOrEmpty(volumeLabel))
                    volumeLabel = LocalParse.getImportPath(file.m_sFull_FileName);
    
                parser.Matches.Add(DBEpisode.cVolumeLabel, volumeLabel);
                
                item = new ListViewItem(file.m_sMatch_FileName);
                item.UseItemStyleForSubItems = true;
                
                progressReporter = new parseResult();
                
                // make sure we have all the necessary data for a full match
                if (!parser.Matches.ContainsKey(DBEpisode.cSeasonIndex) ||
                    !parser.Matches.ContainsKey(DBEpisode.cEpisodeIndex))
                {
                    if (parser.Matches.ContainsKey(DBSeries.cParsedName) &&
                        parser.Matches.ContainsKey(DBOnlineEpisode.cFirstAired))
                    {
                        try{ System.DateTime.Parse(parser.Matches[DBOnlineEpisode.cFirstAired]);}
                        catch (System.FormatException)
                        {
                            nFailed++;
                            progressReporter.failedAirDate = true;
                            progressReporter.success = false;
                            progressReporter.exception = "Air Date is not valid";
                        }
                    }
                    else
                    {
                        progressReporter.success = false;
                        progressReporter.exception = "Parsing failed for: " + file;

                        nFailed++;
                    }

                }
                else
                {
                    // make sure episode & season are properly matched (numerical values)
                    try { Convert.ToInt32(parser.Matches[DBEpisode.cSeasonIndex]); }
                    catch (System.FormatException)
                    {
                        nFailed++;
                        progressReporter.failedSeason = true;
                        progressReporter.success = false;
                        progressReporter.exception += "Season is not numerical ";
                    }
                    try { Convert.ToInt32(parser.Matches[DBEpisode.cEpisodeIndex]); }
                    catch (System.FormatException)
                    {
                        nFailed++;
                        progressReporter.failedEpisode = true;
                        progressReporter.success = false;
                        progressReporter.exception += "Episode is not numerical ";
                    }
                }

                progressReporter.match_filename = file.m_sMatch_FileName;
                progressReporter.full_filename = file.m_sFull_FileName;
                progressReporter.parser = parser;
                progressReporter.PathPair = file;
                if(includeFailed ||progressReporter.success)
                    results.Add(progressReporter);
            }
            MPTVSeriesLog.Write("Finished Local Filename Parsing");
            return results;
        }

        public void AsyncFullParse()
        {
            MPTVSeriesLog.Write("Starting Local Parsing operation - Async: yes", MPTVSeriesLog.LogLevel.Debug);
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MPTVSeriesLog.Write("Finished Parsing operation - Async: True", MPTVSeriesLog.LogLevel.Debug);
            List<parseResult> results = (List<parseResult>)e.Result;
            if (LocalParseCompleted != null)
                LocalParseCompleted.Invoke(results);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            List<parseResult> results = (List<parseResult>)e.UserState;
            if (LocalParseProgress != null)
                LocalParseProgress.Invoke(e.ProgressPercentage, results);
        }
    }

    public class parseResult : IComparable<parseResult>
    {
        public bool success = true;
        public bool failedSeason = false;
        public bool failedEpisode = false;
        public bool failedAirDate = false;
        public string exception;
        public FilenameParser parser;
        public string match_filename;
        public string full_filename;
        public PathPair PathPair;

        private static parseResultComparer comparer = new parseResultComparer();
        public static parseResultComparer Comparer { get { return comparer;}}

        #region IComparable<parseResult> Members
        public int CompareTo(parseResult other)
        {
            return comparer.Compare(this, other);
        }

        #endregion
    }

    public class parseResultComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            ListViewItem xItem = x as ListViewItem;
            ListViewItem yItem = y as ListViewItem;

            if (xItem == null || yItem == null) {
                throw new ArgumentException();
            }

            parseResult xResult = xItem.Tag as parseResult;
            parseResult yResult = yItem.Tag as parseResult;

            if (xResult == null || yResult == null) {
                throw new ArgumentException();
            }

            //sort the parsing failures to the top of the list so they don't get lost in the middle of a big list
            if (xResult.success && !yResult.success) {
                return 1;
            } else if (!xResult.success && yResult.success) {
                return -1;
            }

            return xResult.full_filename.CompareTo(yResult.full_filename);
        }

        #endregion
    }
}
