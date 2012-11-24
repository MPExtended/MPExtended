using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.AccessControl;

namespace MPExtended.Scrapers.MediaManager.FileManagement
{
    public class FolderWatcher
    {
        private String _pathToWatch;
        private List<FileInfo> _filesInFolder = new List<FileInfo>();
        private List<FileInfo> _newCreatedFiles = new List<FileInfo>(); //newly created files

        FileSystemWatcher _folderWatch;

        Thread _fileCreatedCheck;

        bool _folderWatchRunning;

        public bool FolderWatchRunning
        {
            get { return _folderWatchRunning; }
        }

        public List<String> ValidExtensions { get; set; }

        public delegate void NewFileEventHandler(FileSystemEventArgs args);
        public event NewFileEventHandler NewFileCreated;

        public delegate void FileDeletedEventHandler(FileSystemEventArgs args);
        public event FileDeletedEventHandler FileDeleted;

        public delegate void FileRenamedEventHandler(RenamedEventArgs args);
        public event FileRenamedEventHandler FileRenamed;

        public FolderWatcher(String _path, List<String> validExtensions)
        {
            ValidExtensions = validExtensions;
            _pathToWatch = _path;
            string[] files = Directory.GetFiles(_pathToWatch);

            foreach (String file in files)
            {
                if (ValidFileExtension(file))
                {
                    _filesInFolder.Add(new FileInfo(file));
                }
            }

            _folderWatch = new FileSystemWatcher(_pathToWatch);
            _folderWatch.Filter = "*.*";

            _folderWatch.Changed += new FileSystemEventHandler(File_Changed);
            _folderWatch.Created += new FileSystemEventHandler(File_Created);
            _folderWatch.Deleted += new FileSystemEventHandler(File_Deleted);
            _folderWatch.Renamed += new RenamedEventHandler(File_Renamed);
        }

        void File_Renamed(object sender, RenamedEventArgs args)
        {
            if (ValidFileExtension(args.Name))
            {
                if (FileRenamed != null) FileRenamed(args);
            }
        }

        void File_Deleted(object sender, FileSystemEventArgs args)
        {
            if (ValidFileExtension(args.Name))
            {
                if (FileDeleted != null) FileDeleted(args);
            }
        }

        void File_Created(object sender, FileSystemEventArgs args)
        {
            if (ValidFileExtension(args.Name))
            {
                _newCreatedFiles.Add(new FileInfo(args.FullPath));
            }
        }

        void File_Changed(object sender, FileSystemEventArgs args)
        {
            //throw new NotImplementedException();
        }


        ~FolderWatcher()
        {
            _folderWatchRunning = false;
            _fileCreatedCheck.Abort();
        }

        public void Activate()
        {
            _fileCreatedCheck = new Thread(new ThreadStart(FileCheckThread));
            _fileCreatedCheck.Name = "FileCheckThread";
            _folderWatchRunning = true;
            _fileCreatedCheck.Start();
            _folderWatch.EnableRaisingEvents = true;
        }

        public void Resume()
        {
            _folderWatch.EnableRaisingEvents = true;
        }

        public void Pause()
        {
            _folderWatch.EnableRaisingEvents = false;
        }

        public void Deactivate()
        {
            _folderWatch.EnableRaisingEvents = false;
            _folderWatchRunning = false;
            _fileCreatedCheck.Abort();
        }

        private bool ValidFileExtension(string _name)
        {
            foreach (String ext in ValidExtensions)
            {
                if (_name.EndsWith(ext)) return true;
            }
            return false;//extension doesn't match with list of valid exts...
        }

        private void FileCheckThread()
        {
            while (_folderWatchRunning)
            {
                if (_newCreatedFiles.Count != 0)
                {
                    for (int i = 0; i < _newCreatedFiles.Count; i++)
                    {
                        FileStream sf = null;
                        try
                        {
                            string filename = _newCreatedFiles[i].FullName;
                            sf = System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);

                            //at this point the file has been created
                            _filesInFolder.Add(_newCreatedFiles[i]);
                            //NewFileCreated(new FileCreatedEventArgs());

                            NewFileCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, _newCreatedFiles[i].Directory.FullName, _newCreatedFiles[i].Name));
                            _newCreatedFiles.RemoveAt(i);
                        }
                        catch (Exception ex)
                        {
                            //File can't be accessed, copy progress not finished...

                        }
                        finally
                        {
                            if (sf != null) sf.Close();

                        }
                        Thread.Sleep(2000);
                    }
                }
                else
                {
                    Thread.Sleep(10000);
                }
            }
        }

    }
}
