using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using MPExtended.Libraries.Service;

namespace MPExtended.Scrapers.MediaManager.FileManagement
{
    public class FileOperator
    {

        private const int FILE_OPEN_RETRIES = 10; //how often should we try to open the file if it's used by another process

        private Thread _copyThread = null;
        private bool _fileOperationInProgress = false;
        private List<CopyJob> _copyJobs = null;


        public delegate void FileProgressHandler(FileOperationEventArgs _event);
        public event FileProgressHandler FileProgressChanged;

        public delegate void FileOperationFinishedHandler(FileOperationEventArgs _event);
        public event FileOperationFinishedHandler FileOperationFinished;

        public delegate void FileOperationStartedHandler(FileOperationEventArgs _event);
        public event FileOperationStartedHandler FileOperationStarted;


        public FileOperator()
        {
            _copyJobs = new List<CopyJob>();
        }

        public void CopyMediaItemToNewLocation(MediaItem item)
        {
            Log.Debug("Add " + item.File.Name + " to copy queue");
            CopyJob job = CreateCopyJobFromItem(item);
            if (job != null)
            {
                lock (_copyJobs)
                {
                    _copyJobs.Add(job);
                }
            }
            if (!_fileOperationInProgress)
            {
                Log.Debug("Copy queue not running, starting it...");
                StartCopyOperation(null);
            }
        }

        public void CopyMediaItemsToNewLocation(List<MediaItem> items)
        {
            Log.Debug("Add " + items.Count + " episodes to copy queue");

            foreach (MediaItem ef in items)
            {
                CopyJob job = CreateCopyJobFromItem(ef);
                if (job != null)
                {
                    lock (_copyJobs)
                    {
                        _copyJobs.Add(job);
                    }
                }
            }

            if (!_fileOperationInProgress)
            {
                Log.Debug("Copy queue not running, starting it...");
                StartCopyOperation(null);
            }
        }

        private CopyJob CreateCopyJobFromItem(MediaItem item)
        {
            CopyJob job = new CopyJob();
            job.Item = item;
            job.SourceFile = item.File;
            //TODO: create destination based on media item
            //job.DestinationDirectory = new DirectoryInfo();
            //job.NewFileName = newFileName;
            job.DeleteSource = true;
            job.Backup = true;
            return job;
        }

        public void MoveFileToLocation(FileInfo file, DirectoryInfo location, bool deleteSource)
        {
            Log.Debug("Starting copy thread for " + file.Name + " with 4 retries");
            new Thread(new ParameterizedThreadStart(MoveFileToLocation)).Start(new object[] { file, location, 0, 4, deleteSource });
        }

        private void MoveFileToLocation(object data)
        {
            object[] parameters = (object[])data;
            FileInfo _file = (FileInfo)parameters[0];
            DirectoryInfo _location = (DirectoryInfo)parameters[1];
            int count = (int)parameters[2];
            int max = (int)parameters[3];

            if (count < max)
            {
                Log.Info("Attempt nr. " + count + " to copy " + _file + " to " + _location);
                try
                {
                    if (!_location.Exists)
                    {
                        Log.Debug("Location " + _location.FullName + " doesn't exist -> creating it");
                        try
                        {
                            _location.Create();
                        }
                        catch (Exception)
                        {
                            Log.Debug("Location " + _location.FullName + " couldn't be created");
                        }
                    }
                    File.Move(_file.FullName, _location.FullName + "\\" + _file.Name);
                }
                catch (Exception ex)
                {
                    Log.Info("Attempt nr. " + count + " failed");
                    Log.Debug(ex.ToString());
                    MoveFileToLocation(new object[] { _file, _location, count + 1, max });
                }
            }
            else
            {
                Log.Info("Tried " + max + " times to copy " + _location + ", giving up now");
            }
        }

        public void StartCopyOperation(object data)
        {
            _fileOperationInProgress = true;

            ThreadStart threadStarter = new ThreadStart(CopyEpisodeFiles);
            _copyThread = new Thread(threadStarter);
            _copyThread.Name = "CopyThread";
            _copyThread.Start();


        }

        private void CopyEpisodeFiles()
        {
            try
            {
                CopyJob currentJob = null;
                lock (_copyJobs)
                {
                    if (_copyJobs.Count > 0 && _copyJobs[0] != null)
                    {
                        currentJob = _copyJobs[0];
                        Log.Debug("Take " + currentJob.SourceFile.Name + " from queue to start it");
                    }
                    else
                    {
                        Log.Warn("Tried to take copyjob from list but list empty");
                    }
                }

                while (currentJob != null)
                {
                    try
                    {
                        //check if file exists
                        if (currentJob.SourceFile.Exists)
                        {

                            //check if destination drive has enough space left
                            long freeSpace = 0;
                            String driveLetter = "?";
                            bool found = false;
                            Log.Debug("Listing all drives");

                            foreach (DriveInfo d in System.IO.DriveInfo.GetDrives())
                            {
                                if (currentJob.DestinationDirectory.Root.Name.ToLower().StartsWith(d.Name.ToLower()) && d.IsReady)
                                {
                                    Log.Debug("Checking free space for " + driveLetter);
                                    driveLetter = d.Name;
                                    freeSpace = d.AvailableFreeSpace;
                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                            {
                                Log.Debug("Space left on " + driveLetter + ": " + FileUtils.FormatBytes(freeSpace));
                                if (freeSpace > currentJob.SourceFile.Length)
                                {
                                    if (!currentJob.DestinationDirectory.Exists)
                                    {//No directory was found, create new one ("season.xx")
                                        Log.Info("Directory " + currentJob.DestinationDirectory.ToString() + " not existing yet, creating it");
                                        try
                                        {
                                            currentJob.DestinationDirectory.Create();
                                        }
                                        catch (Exception)
                                        {
                                            Log.Error("Couldn't create " + currentJob.DestinationDirectory.ToString() + " aborting file copy");
                                            break;
                                        }
                                    }

                                    FileInfo newFile = null;
                                    if (currentJob.NewFileName != null)
                                    {
                                        newFile = new FileInfo(currentJob.DestinationDirectory.FullName + "\\" + currentJob.NewFileName);
                                    }
                                    else
                                    {
                                        newFile = new FileInfo(currentJob.DestinationDirectory.FullName + "\\" + currentJob.SourceFile.Name);
                                    }

                                    if (File.Exists(newFile.FullName))//File allready exists, do not copy for now
                                    {
                                        FileOperationFinished(new FileOperationEventArgs(currentJob.Item, currentJob.SourceFile, newFile, FileCopyProgress.existing, FileOperationEventArgs.OperationType.Copy));
                                    }
                                    else
                                    {
                                        //trigger event to signal a new file operation
                                        FileInfo oldFile = new FileInfo(currentJob.SourceFile.FullName);

                                        if (FileOperationStarted != null)
                                        {
                                            FileOperationStarted(new FileOperationEventArgs(currentJob.Item, oldFile, newFile, FileCopyProgress.copying, FileOperationEventArgs.OperationType.Copy));
                                        }

                                        Log.Debug("Attempt to copy " + oldFile + " to " + newFile);
                                        bool finished = false;
                                        try
                                        {
                                            finished = Copy(oldFile, newFile, currentJob.Item);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error("Copy(oldFile, newFile) produced error: " + ex.ToString());
                                        }
                                        Log.Debug("Copy " + oldFile + " to " + newFile + " finished " + (finished ? "successful" : "unsuccessful"));

                                        //trigger event to signal a finished file operation
                                        if (FileOperationFinished != null)
                                        {
                                            if (finished)
                                            {
                                                Log.Debug("Finished copying episode " + newFile.FullName);
                                                FileOperationFinished(new FileOperationEventArgs(currentJob.Item, oldFile, newFile, FileCopyProgress.moved, FileOperationEventArgs.OperationType.Copy));
                                            }
                                            else
                                            {//File Operation failed
                                                //reset to old file since we havn't actually moved the file
                                                Log.Debug("Couldn't finish copying episode " + newFile.FullName);
                                                currentJob.Item.File = oldFile;
                                                FileOperationFinished(new FileOperationEventArgs(currentJob.Item, oldFile, newFile, FileCopyProgress.failed, FileOperationEventArgs.OperationType.Copy));
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    Log.Warn("The left space on  " + driveLetter + "(" + freeSpace + ") is not sufficient for " + currentJob.SourceFile.FullName);
                                    if (FileOperationFinished != null)
                                    {
                                        FileOperationFinished(new FileOperationEventArgs(currentJob.Item, currentJob.SourceFile, null, FileCopyProgress.nospaceleft, FileOperationEventArgs.OperationType.Copy));
                                    }
                                }
                            }
                            else
                            {
                                Log.Warn("Couldn't get drive Info for  " + currentJob.DestinationDirectory.Root.FullName);
                                if (FileOperationFinished != null)
                                {
                                    FileOperationFinished(new FileOperationEventArgs(currentJob.Item, currentJob.SourceFile, null, FileCopyProgress.failed, FileOperationEventArgs.OperationType.Copy));
                                }
                            }
                        }
                        else
                        {//file doesn't exist
                            Log.Warn("The file " + currentJob.SourceFile.FullName + " doesn't exist");
                            if (FileOperationFinished != null)
                            {
                                FileOperationFinished(new FileOperationEventArgs(currentJob.Item, currentJob.SourceFile, null, FileCopyProgress.doesntexist, FileOperationEventArgs.OperationType.Copy));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Fuck, uncaught error while copying file:");
                        Log.Debug(ex.ToString());
                    }

                    lock (_copyJobs)
                    {
                        //Get next episode to copy, remove finished one from queue
                        Log.Debug("Remove " + _copyJobs[0].SourceFile + " from filelist");
                        _copyJobs.RemoveAt(0);
                        if (_copyJobs.Count > 0)
                        {
                            Log.Debug("Take " + _copyJobs[0].SourceFile + " from queue to start it");
                            currentJob = _copyJobs[0];
                        }
                        else
                        {
                            Log.Debug("No more episodes left in queue");
                            currentJob = null; //loop will break
                        }
                    }
                }
                _fileOperationInProgress = false;
            }
            catch (Exception ex)
            {
                Log.Error("Error during file copy operation: ");
                Log.Debug(ex.ToString());
            }
        }

        private bool Copy(FileInfo _oldFile, FileInfo _newFile, MediaItem item)
        {
            DateTime startTime = DateTime.Now;
            //Create Buffer
            byte[] buff = new byte[1024];

            //Create FileStream for read/write
            FileStream fout = null;
            FileStream fin = null;
            bool success = false;
            for (int i = 0; i < FILE_OPEN_RETRIES; i++)
            {
                try
                {
                    fin = new FileStream(_oldFile.FullName, FileMode.Open, FileAccess.Read);
                    success = true;
                }
                catch (IOException)
                {
                    Log.Warn("Error Opening Input File, retrying");
                    Thread.Sleep(500);
                }
            }
            if (!success)
            {//Couldn't open the input file (most likely used by another process)
                Log.Error("Couldn't open Input File after " + FILE_OPEN_RETRIES + "retries");
                success = false;
            }

            if (success)
            {
                try
                {
                    fout = new FileStream(_newFile.FullName, FileMode.Create, FileAccess.Write);
                }
                catch (IOException ex)
                {
                    Log.Error(ex.Message + "\nError Opening Output File");
                    success = false;
                }
            }

            if (success)
            {
                try
                {
                    long percentageOld = 0;
                    long fileSize = _oldFile.Length;
                    int bytesRead = buff.Length;
                    long offset = 0;
                    while (bytesRead == buff.Length)
                    {
                        bytesRead = fin.Read(buff, 0, buff.Length);

                        fout.Write(buff, 0, bytesRead);
                        offset += bytesRead;

                        //Progress handling
                        if (fileSize != 0)
                        {
                            long percentage = offset * 100 / fileSize;
                            if (percentage >= percentageOld + 2)
                            {
                                if (FileProgressChanged != null)
                                {
                                    FileProgressChanged(new FileOperationEventArgs(item, _oldFile, _newFile,
                                                                                   FileCopyProgress.copying,
                                                                                   percentage, FileOperationEventArgs.OperationType.Copy));
                                }
                                percentageOld = percentage;
                            }
                        }

                    }
                }
                catch (IOException ex)
                {
                    Log.Error("Error copying the file: ");
                    Log.Debug(ex.ToString());
                    success = false;
                }
            }

            //File Copy is finished
            try
            {
                if (fin != null) fin.Close();
                if (fout != null) fout.Close();
                if (!success && File.Exists(_newFile.FullName))
                {
                    File.Delete(_newFile.FullName);
                }

            }
            catch (IOException ex)
            {
                Log.Error(ex.Message);
                return false;
            }

            TimeSpan duration = DateTime.Now.Subtract(startTime);
            Log.Debug("Duration file transfer: " + duration.Seconds + " seconds");
            return success;
        }
    }
}
