using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.FileSystem;
using System.IO;

namespace MPExtended.PlugIns.MAS.LocalFileSystem
{
    public class LocalFileSystem : IFileSystemProvider
    {
        private Dictionary<string, WebDriveBasic> _drives = new Dictionary<string, WebDriveBasic>();
        private Dictionary<string, WebFolderBasic> _folders = new Dictionary<string, WebFolderBasic>();
        private Dictionary<string, WebFileBasic> _files = new Dictionary<string, WebFileBasic>();

        public IEnumerable<WebDriveBasic> GetLocalDrives()
        {
            List<WebDriveBasic> drives = new List<WebDriveBasic>();
            // Store in a string array
            var localDrives = System.IO.DriveInfo.GetDrives();
            // Loop into the string array
            foreach (var drive in localDrives)
            {
                WebDriveBasic webDrive = new WebDriveBasic();
                webDrive.Id = EncodeTo64(drive.RootDirectory.FullName);
                webDrive.Name = drive.Name;
                webDrive.Path = drive.RootDirectory.FullName;

                if (_drives.Keys.Contains(EncodeTo64(drive.RootDirectory.FullName)) == false)
                {
                    _drives.Add(webDrive.Id, webDrive);
                }
                drives.Add(webDrive);
            }
            return drives;
        }

        public IEnumerable<WebFileBasic> GetFilesByPath(string id)
        {
            List<WebFileBasic> localFiles = new List<WebFileBasic>();

            string path = "";

            if (_drives.Keys.Contains(id))
            {
                WebDriveBasic drive;
                _drives.TryGetValue(id, out drive);
                path = drive.Path;

            }
            else if (_folders.Keys.Contains(id))
            {
                WebFolderBasic folder;
                _folders.TryGetValue(id, out folder);
                path = folder.Path;
            }
            if (!String.IsNullOrEmpty(path))
            {
                var files = new DirectoryInfo(path).GetFiles();

                foreach (var file in files)
                {
                    WebFileBasic webFile = new WebFileBasic();
                    webFile.Name = file.Name;
                    webFile.Path = file.FullName;
                    webFile.DateAdded = file.CreationTime;
                    webFile.Id = EncodeTo64(file.FullName);

                    if (_files.Keys.Contains(webFile.Id) == false)
                    {
                        _files.Add(webFile.Id, webFile);
                    }
                    localFiles.Add(webFile);
                }
            }
            return localFiles;

        }

        public IEnumerable<WebFolderBasic> GetFoldersByPath(string id)
        {
            List<WebFolderBasic> folders = new List<WebFolderBasic>();

            string path = "";

            if (_drives.Keys.Contains(id))
            {
                WebDriveBasic drive;
                _drives.TryGetValue(id, out drive);
                path = drive.Path;

            }
            else if (_folders.Keys.Contains(id))
            {
                WebFolderBasic folder;
                _folders.TryGetValue(id, out folder);
                path = folder.Path;
            }
            if (!String.IsNullOrEmpty(path))
            {
                var localFolder = new DirectoryInfo(path).GetDirectories();

                foreach (var dir in localFolder)
                {
                    WebFolderBasic folder = new WebFolderBasic();
                    folder.Name = dir.Name;
                    folder.Path = dir.FullName;
                    folder.DateAdded = dir.CreationTime;
                    folder.Id = EncodeTo64(dir.FullName);
                    if (_folders.Keys.Contains(folder.Id) == false)
                    {
                        _folders.Add(folder.Id, folder);
                    }
                    folders.Add(folder);
                }
            }
            return folders;
        }

        public System.IO.Stream GetFile(string id)
        {
            WebFileBasic file;
            _files.TryGetValue(id, out file);
            if (file != null)
            {
                return new FileStream(file.Path, FileMode.Open , FileAccess.Read);
            }
            return null;
        }

        static private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        static private string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }


    }
}
