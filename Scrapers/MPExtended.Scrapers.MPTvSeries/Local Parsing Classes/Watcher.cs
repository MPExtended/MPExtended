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
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

class MPR
{
    const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
    const int REMOTE_NAME_INFO_LEVEL = 0x00000002;

    const int ERROR_MORE_DATA = 234;
    const int NOERROR = 0;

    [DllImport("mpr.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    static extern int WNetGetUniversalName(
        string lpLocalPath,
        [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
        IntPtr lpBuffer,
        [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

    public static string GetUniversalName(string localPath)
    {
        // The return value.
        string retVal = null;

        // The pointer in memory to the structure.
        IntPtr buffer = IntPtr.Zero;

        // Wrap in a try/catch block for cleanup.
        try
        {
            // First, call WNetGetUniversalName to get the size.
            int size = 0;

            // Make the call.
            // Pass IntPtr.Size because the API doesn't like null, even though
            // size is zero.  We know that IntPtr.Size will be
            // aligned correctly.
            int apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size, ref size);

            // If the return value is not ERROR_MORE_DATA, then
            // raise an exception.
            if (apiRetVal != ERROR_MORE_DATA)
                // Throw an exception.
                throw new Win32Exception(apiRetVal);

            // Allocate the memory.
            buffer = Marshal.AllocCoTaskMem(size);

            // Now make the call.
            apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

            // If it didn't succeed, then throw.
            if (apiRetVal != NOERROR)
                // Throw an exception.
                throw new Win32Exception(apiRetVal);

            // Now get the string.  It's all in the same buffer, but
            // the pointer is first, so offset the pointer by IntPtr.Size
            // and pass to PtrToStringAuto.
            retVal = Marshal.PtrToStringAnsi(new IntPtr(buffer.ToInt64() + IntPtr.Size));
        }
        finally
        {
            // Release the buffer.
            Marshal.FreeCoTaskMem(buffer);
        }

        // First, allocate the memory for the structure.

        // That's all folks.
        return retVal;
    }
}


namespace WindowPlugins.GUITVSeries
{
    public enum WatcherItemType
    {
        Added,
        Deleted
    }

    public class WatcherItem
    {
        public String m_sFullPathFileName;
        public String m_sParsedFileName;
        public WatcherItemType m_type;        
		private FileInfo fileInfo = null;

        public WatcherItem(FileSystemWatcher watcher, RenamedEventArgs e, bool bOldName)
        {
            if (bOldName)
            {
                m_sFullPathFileName = e.OldFullPath;
                m_sParsedFileName = m_sFullPathFileName.Substring(watcher.Path.Length).TrimStart('\\');
                m_type = WatcherItemType.Deleted;
                MPTVSeriesLog.Write("File monitor: " + m_sParsedFileName + " " + m_type);
            }
            else
            {
                m_sFullPathFileName = e.FullPath;
                m_sParsedFileName = m_sFullPathFileName.Substring(watcher.Path.Length).TrimStart('\\');                
                fileInfo = new FileInfo(m_sFullPathFileName);
                m_type = WatcherItemType.Added;
                MPTVSeriesLog.Write("File monitor: " + m_sParsedFileName + " " + m_type);
            }
        }

        public WatcherItem(FileSystemWatcher watcher, FileSystemEventArgs e)
        {
            m_sFullPathFileName = e.FullPath;
            m_sParsedFileName = m_sFullPathFileName.Substring(watcher.Path.Length).TrimStart('\\');
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                    m_type = WatcherItemType.Deleted;
                    break;

                default:
                    m_type = WatcherItemType.Added;                    
                    fileInfo = new FileInfo(m_sFullPathFileName);
                    break;
            }
            MPTVSeriesLog.Write("File monitor: " + m_sParsedFileName + " " + m_type);
        }

        public WatcherItem(PathPair file, WatcherItemType type)
        {
            m_type = type;
            m_sFullPathFileName = file.m_sFull_FileName;
            m_sParsedFileName = file.m_sMatch_FileName;           
            if (type == WatcherItemType.Added)
                fileInfo = new FileInfo(file.m_sFull_FileName);
            MPTVSeriesLog.Write("File monitor: " + m_sParsedFileName + " " + m_type);
        }
     
        bool IsLocked(FileInfo fileInfo)
        {
            FileStream stream = null;
            try
            {
                stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null) stream.Close();
            }

            return false;
        }
       
        public bool IsLocked()
        {
            if (fileInfo != null) 
            {
                return IsLocked(fileInfo);
            }
            return false;
        }
    };

    public class Watcher
    {
        public BackgroundWorker worker = new BackgroundWorker();
        List<String> m_WatchedFolders = new List<String>();
        int m_nScanLapse; // number of minutes between scans
        
        List<String> m_ScannedFolders = new List<String>();
        List<PathPair> m_PreviousScan = new List<PathPair>();
        List<PathPair> m_PreviousScanRemovable = new List<PathPair>();

        List<System.IO.FileSystemWatcher> m_watchersList = new List<System.IO.FileSystemWatcher>();
        List<WatcherItem> m_modifiedFilesList = new List<WatcherItem>();
        volatile bool refreshWatchers = false;
        public delegate void WatcherProgressHandler(int nProgress, List<WatcherItem> modifiedFilesList);

        /// <summary>
        /// This will be triggered once all the SeriesAndEpisodeInfo has been parsed completely.
        /// </summary>
        public event WatcherProgressHandler WatcherProgress;

        public Watcher(List<String> WatchedFolders, int nScanLapse)
        {
            MPTVSeriesLog.Write("File Watcher: Creating new File System Watcher", MPTVSeriesLog.LogLevel.Normal);

            m_nScanLapse = nScanLapse;

            foreach (String folder in WatchedFolders) {
                string sRoot = System.IO.Path.GetPathRoot(folder);
                try
                {
                    DriveInfo info = new DriveInfo(sRoot);
                    
                    if (info.DriveType == DriveType.CDRom) {
                        // do nothing as filesystemwatchers do nothing for cd or dvd drives
                        MPTVSeriesLog.Write(string.Format("File Watcher: Skipping CD/DVD drive: {0}", sRoot), MPTVSeriesLog.LogLevel.Normal);
                    }
                    else if (info.DriveType==DriveType.Network) {
                        //string sUNCPath=MPR.GetUniversalName(folder);

                        MPTVSeriesLog.Write(string.Format("File Watcher: Adding watcher on folder: {0}", folder), MPTVSeriesLog.LogLevel.Normal);
                        m_ScannedFolders.Add(folder);
                    }
                    else {
                        MPTVSeriesLog.Write(string.Format("File Watcher: Adding watcher on folder: {0}", folder), MPTVSeriesLog.LogLevel.Normal);
                        m_WatchedFolders.Add(folder);
                    }
                }
                catch (System.ArgumentException)
                {
                	// this has to be a UNC path
                    MPTVSeriesLog.Write(string.Format("File Watcher: Adding watcher on folder: {0}", folder), MPTVSeriesLog.LogLevel.Normal);
                    m_ScannedFolders.Add(folder);
                }
            }
            
            DeviceManager.OnVolumeInserted += OnVolumeInsertedRemoved;
            DeviceManager.OnVolumeRemoved += OnVolumeInsertedRemoved;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_Completed);
            worker.DoWork += new DoWorkEventHandler(workerWatcher_DoWork);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (WatcherProgress != null) // only if any subscribers exist
                WatcherProgress.Invoke(e.ProgressPercentage, e.UserState as List<WatcherItem>);
        }

        void worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            MPTVSeriesLog.Write("Successfully stopped File System Watchers.");
        }

        public void StartFolderWatch()
        {
            // start the thread that is going to handle the addition in the db when files change
            worker.RunWorkerAsync();
        }

        public void StopFolderWatch()
        {
            MPTVSeriesLog.Write("Stopping File System Watchers...");
            if (worker.IsBusy)
                worker.CancelAsync();
        }

        void removeFromModifiedFilesList(string filePath, WatcherItemType type, bool isFolder)
        {
            List<WatcherItem> watcherItemsRemove = new List<WatcherItem>();
            string completeFilePath = filePath;
            if (isFolder) completeFilePath = completeFilePath + "\\";
            
            foreach (WatcherItem watcherItem in m_modifiedFilesList)
            {
                if (watcherItem.m_sFullPathFileName.StartsWith(filePath) && watcherItem.m_type == type)
                    watcherItemsRemove.Add(watcherItem);
            }
            foreach (WatcherItem watcherItem in watcherItemsRemove)
                m_modifiedFilesList.Remove(watcherItem);
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            MPTVSeriesLog.Write("File Watcher: Renamed event: " + e.OldFullPath + " to " + e.FullPath);

            List<PathPair> filesToRemove = new List<PathPair>();
            List<PathPair> filesToAdd = new List<PathPair>();
            bool isDirectoryRename = false;

            if (Directory.Exists(e.FullPath)) 
            {
                isDirectoryRename = true;

                List<string> folder = new List<string>();
                folder.Add(e.FullPath);
                filesToAdd = Filelister.GetFiles(folder);
                
                foreach (PathPair pathPair in filesToAdd)
                    filesToRemove.Add(new PathPair(pathPair.m_sMatch_FileName, pathPair.m_sFull_FileName.Replace(e.FullPath, e.OldFullPath)));
            }

            // rename: delete the old, add the new
            lock (m_modifiedFilesList)
            {
                if (isDirectoryRename)
                {
                    foreach (PathPair pathPair in filesToRemove)
                    {
                        removeFromModifiedFilesList(pathPair.m_sFull_FileName, WatcherItemType.Added, false);
                        m_modifiedFilesList.Add(new WatcherItem(pathPair, WatcherItemType.Deleted));
                    }
                    foreach (PathPair pathPair in filesToAdd)
                    {
                        removeFromModifiedFilesList(pathPair.m_sFull_FileName, WatcherItemType.Deleted, false);
                        m_modifiedFilesList.Add(new WatcherItem(pathPair, WatcherItemType.Added));
                    }
                }
                else
                {
                    String sOldExtention = System.IO.Path.GetExtension(e.OldFullPath);
                    if (MediaPortal.Util.Utils.VideoExtensions.IndexOf(sOldExtention) != -1)
                    {
                        removeFromModifiedFilesList(e.OldFullPath, WatcherItemType.Added, false);
                        m_modifiedFilesList.Add(new WatcherItem(sender as FileSystemWatcher, e, true));
                    }
                    String sNewExtention = System.IO.Path.GetExtension(e.FullPath);
                    if (MediaPortal.Util.Utils.VideoExtensions.IndexOf(sNewExtention) != -1)
                    {
                        removeFromModifiedFilesList(e.FullPath, WatcherItemType.Deleted, false);
                        m_modifiedFilesList.Add(new WatcherItem(sender as FileSystemWatcher, e, false));
                    }
                }
            }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {           
            MPTVSeriesLog.Write("File Watcher: Changed event: " + e.FullPath);

            List<PathPair> filesChanged = new List<PathPair>();
            bool isDirectoryChange = false;

            if (Directory.Exists(e.FullPath))
            {
                isDirectoryChange = true;

                List<string> folder = new List<string>();
                folder.Add(e.FullPath);
                filesChanged = Filelister.GetFiles(folder);
            }
            
            // a file has changed! created, not created, whatever. Just add it to our list. 
            // we only process this list once in a while
            lock (m_modifiedFilesList)
            {
                if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    removeFromModifiedFilesList(e.FullPath, WatcherItemType.Added, true);
                    
                    SQLCondition condition = new SQLCondition(new DBEpisode(), DBEpisode.cFilename, e.FullPath + "\\%", SQLConditionType.Like);
                    List<DBEpisode> dbepisodes = DBEpisode.Get(condition, false);
                    if (dbepisodes != null && dbepisodes.Count > 0)
                    {
                        foreach (DBEpisode dbepisode in dbepisodes)
                        {
                            m_modifiedFilesList.Add(new WatcherItem(new PathPair(dbepisode[DBEpisode.cFilename].ToString().Substring(e.FullPath.Length).TrimStart('\\'), dbepisode[DBEpisode.cFilename]), WatcherItemType.Deleted));
                        }
                    }
                }

                if (isDirectoryChange)
                {
                    foreach (PathPair pathPair in filesChanged)
                    {
                        removeFromModifiedFilesList(pathPair.m_sFull_FileName, WatcherItemType.Deleted, false);
                        m_modifiedFilesList.Add(new WatcherItem(pathPair, WatcherItemType.Added));
                    }
                }
                else
                {
                    /* duplicates are removed later
                    foreach (WatcherItem item in m_modifiedFilesList)
                    {
                        if (item.m_sFullPathFileName == e.FullPath)
                            return;
                    }
                    */

                    String sExtention = System.IO.Path.GetExtension(e.FullPath);
                    if (MediaPortal.Util.Utils.VideoExtensions.IndexOf(sExtention) != -1)
                    {
                        if (e.ChangeType == WatcherChangeTypes.Deleted)
                            removeFromModifiedFilesList(e.FullPath, WatcherItemType.Added, false);
                        else
                            removeFromModifiedFilesList(e.FullPath, WatcherItemType.Deleted, false);

                        m_modifiedFilesList.Add(new WatcherItem(sender as FileSystemWatcher, e));
                    }
                }
            }
        }

        void setUpWatches()
        {            
            if (m_watchersList.Count > 0) 
            {
                MPTVSeriesLog.Write("File Watcher: Cleaning up File System Watchers", MPTVSeriesLog.LogLevel.Normal);

                // do some cleanup first, remove the existing watchers
                foreach (FileSystemWatcher watcher in m_watchersList)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Changed -= new FileSystemEventHandler(watcher_Changed);
                    watcher.Created -= new FileSystemEventHandler(watcher_Changed);
                    watcher.Deleted -= new FileSystemEventHandler(watcher_Changed);
                    watcher.Renamed -= new RenamedEventHandler(watcher_Renamed);
                    watcher.Error -= new ErrorEventHandler(watcher_Error);
                }
                m_watchersList.Clear();
            }
            
            // go through all enabled import folders, and add a watchfolder on it
            foreach (String sWatchedFolder in m_WatchedFolders)
            {
                if (Directory.Exists(sWatchedFolder))
                {
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    // ZFLH watch for all types of file notification, filter in the event notification
                    // from MSDN, filter doesn't change the amount of stuff looked at internally
                    watcher.Path = sWatchedFolder;
                    watcher.IncludeSubdirectories = true;
                    watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
                    watcher.Changed += new FileSystemEventHandler(watcher_Changed);
                    watcher.Created += new FileSystemEventHandler(watcher_Changed);
                    watcher.Deleted += new FileSystemEventHandler(watcher_Changed);
                    watcher.Renamed += new RenamedEventHandler(watcher_Renamed);                    
                    watcher.Error += new ErrorEventHandler(watcher_Error);
                    watcher.EnableRaisingEvents = true;
                    m_watchersList.Add(watcher);
                }
            }
            refreshWatchers = false;
        }

        void watcher_Error(object sender, ErrorEventArgs e)
        {
            MPTVSeriesLog.Write("File Watcher: Error event: " + e.GetException().Message);
            refreshWatchers = true;
        }

        static List<WatcherItem> removeDuplicates(List<WatcherItem> inputList)
        {
            Dictionary<string, int> uniqueStore = new Dictionary<string, int>();
            List<WatcherItem> finalList = new List<WatcherItem>();

            MPTVSeriesLog.Write("Remove duplicates from inputList, starting count: " + inputList.Count, MPTVSeriesLog.LogLevel.DebugSQL);

            foreach (WatcherItem currValue in inputList)
            {
                string key = currValue.m_sFullPathFileName;
                switch (currValue.m_type)
                {
                    case WatcherItemType.Deleted:
                        key += " DELETED";
                        break;
                    case WatcherItemType.Added:
                        key += " ADDED";
                        break;
                }

                if (!uniqueStore.ContainsKey(key))
                {
                    uniqueStore.Add(key, 0);
                    finalList.Add(currValue);
                }
            }

            MPTVSeriesLog.Write("Removed duplicates from inputList, final count: " + inputList.Count, MPTVSeriesLog.LogLevel.DebugSQL);
            return finalList;
        }

        void signalModifiedFiles()
        {
            try
            {
                lock (m_modifiedFilesList)
                {
                    if (m_modifiedFilesList.Count > 0)
                    {
                        m_modifiedFilesList = removeDuplicates(m_modifiedFilesList);

                        MPTVSeriesLog.Write("File Watcher: Signaling " + m_modifiedFilesList.Count + " modified files");
                        List<WatcherItem> outList = new List<WatcherItem>();
                       
                        // leave locked files in the m_modifiedFilesList
                        foreach (WatcherItem watcherItem in m_modifiedFilesList)
                        {
                            if (!watcherItem.IsLocked())
                            {
                                outList.Add(watcherItem);
                            }
                            else
                            {
                                MPTVSeriesLog.Write("File " + watcherItem.m_sParsedFileName + " is locked, leaving for later parsing!", MPTVSeriesLog.LogLevel.Debug);
                            }
                        }
                        // clear m_modifiedFilesList
                        foreach (WatcherItem watcherItem in outList)
                        {
                            m_modifiedFilesList.Remove(watcherItem);
                        }

                        // go over the modified files list once in a while & update                        
                        //outList.AddRange(m_modifiedFilesList);
                        //m_modifiedFilesList.Clear();
                        if (outList.Count > 0)
                            worker.ReportProgress(0, outList);
                    }
                }
            }
            catch (Exception exp)
            {
                MPTVSeriesLog.Write("File Watcher: Exception happened in Signal Modified Files: " + exp.Message);
            }
        }
        
        List<String> GetDeviceManagerWatchedFolders()
        {                        
            List<String> result = new List<String>();
            if (DeviceManager.watchedDrives == null) return result;

            foreach (DBImportPath importPath in DBImportPath.GetAll())
            {
                string sRoot = System.IO.Path.GetPathRoot(importPath[DBImportPath.cPath]);
                string importPathString = importPath[DBImportPath.cPath];
                if (!String.IsNullOrEmpty(importPathString) && (importPath[DBImportPath.cEnabled] != 0) && !String.IsNullOrEmpty(sRoot))
                {
                    foreach (String drive in DeviceManager.watchedDrives)
                    {
                        if (importPathString.StartsWith(drive) && !result.Contains(importPathString))
                            result.Add(importPathString);
                    }
                }
            }
            return result;
        }
        
        void OnVolumeInsertedRemoved(string volume, string serial)
        {
            MPTVSeriesLog.Write("On Volume Inserted or Removed: " + volume);
            
            List<String> folders = new List<String>();

            foreach (DBImportPath importPath in DBImportPath.GetAll())
            {
                string sRoot = System.IO.Path.GetPathRoot(importPath[DBImportPath.cPath]);
                if ((importPath[DBImportPath.cEnabled] != 0) && !String.IsNullOrEmpty(sRoot) && sRoot.ToLower().StartsWith(volume.ToLower()))
                {                    
					MPTVSeriesLog.Write("Adding for import or remove: " + importPath[DBImportPath.cPath]);
                    folders.Add(importPath[DBImportPath.cPath]); 
                }
            }

            if (folders.Count > 0)
            {
                List<PathPair> m_PreviousScanRemovableTemp = new List<PathPair>();
                m_PreviousScanRemovableTemp.AddRange(m_PreviousScanRemovable);
                foreach (String pair in folders)
                {
                    m_PreviousScanRemovableTemp.RemoveAll(item => !item.m_sFull_FileName.StartsWith(pair));
                }
                foreach (PathPair pair in m_PreviousScanRemovableTemp)
                {
                    m_PreviousScanRemovable.RemoveAll(item => item.m_sFull_FileName == pair.m_sFull_FileName);
                }

                DoFileScan(folders, ref m_PreviousScanRemovableTemp);

                m_PreviousScanRemovable.AddRange(m_PreviousScanRemovableTemp);
            }
        }
        
        void DoFileScan()
        {
            DoFileScan(m_ScannedFolders, ref m_PreviousScan);
        }

        void DoFileScan(List<String> scannedFolders, ref List<PathPair> previousScan)
        {

            MPTVSeriesLog.Write("File Watcher: Performing File Scan on Import Paths for changes", MPTVSeriesLog.LogLevel.Normal);
            
            // Check if Fullscreen Video is active as this can cause stuttering/dropped frames
            if (!DBOption.GetOptions(DBOption.cImport_ScanWhileFullscreenVideo) &&  Helper.IsFullscreenVideo) {
                MPTVSeriesLog.Write("File Watcher: Fullscreen Video has been detected, aborting file scan");
                return;
            }
            
            List<PathPair> newScan = Filelister.GetFiles(scannedFolders);

            List<PathPair> addedFiles = new List<PathPair>();
            addedFiles.AddRange(newScan);
            
            foreach (PathPair pair in previousScan)
            {
                addedFiles.RemoveAll(item => item.m_sFull_FileName == pair.m_sFull_FileName);
            }

            List<PathPair> removedFiles = new List<PathPair>();
            
            removedFiles.AddRange(previousScan);
            foreach (PathPair pair in newScan) {
                removedFiles.RemoveAll(item => item.m_sFull_FileName == pair.m_sFull_FileName);
            }

            lock (m_modifiedFilesList)
            {
                foreach (PathPair pair in addedFiles)
                    m_modifiedFilesList.Add(new WatcherItem(pair, WatcherItemType.Added));

                foreach (PathPair pair in removedFiles)
                    m_modifiedFilesList.Add(new WatcherItem(pair, WatcherItemType.Deleted));
            }
            
            previousScan = newScan;

            MPTVSeriesLog.Write("File Watcher: Finished File Scan on Import Paths", MPTVSeriesLog.LogLevel.Normal);
        }

        void workerWatcher_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;

            // delay the start of file monitoring for a small period
            // this doesnt really need a config setting but can be manually overridden in DB option
            // purpose of the delay is to allow network devices to get ready after fresh boot from windows
            // and resume from standby...benifits UNC shares as Mapped drives are picked up from Device Manager events.
            // also DoFileScan can be expensive, so we dont want to do this on startup while other plugins/background tasks
            // are running as well.
           Thread.Sleep((int)DBOption.GetOptions(DBOption.cDelayImportPathMonitoringValue)*1000);
           
            MPTVSeriesLog.Write("File Watcher: Starting File System Watcher Background Task", MPTVSeriesLog.LogLevel.Normal);
            setUpWatches();

            // do the initial scan
            m_PreviousScan = Filelister.GetFiles(m_ScannedFolders);
            m_PreviousScanRemovable = Filelister.GetFiles(GetDeviceManagerWatchedFolders());

            DateTime timeLastScan = DateTime.Now;

            // then start the watcher loop
            while (!worker.CancellationPending)
            {               
               TimeSpan tsUpdate = DateTime.Now - timeLastScan;
                if ((int)tsUpdate.TotalMinutes >= m_nScanLapse) 
                {
                    timeLastScan = DateTime.Now;
                    DoFileScan();
                }
           
                signalModifiedFiles();

                if (refreshWatchers)
                    setUpWatches();
       
                Thread.Sleep(1000);
            }
        }
    };

}

