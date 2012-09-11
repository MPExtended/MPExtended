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
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using MPExtended.Libraries.Service.Config.Upgrade;

namespace MPExtended.Libraries.Service.Config
{
    public interface IConfigurationSerializer
    {
        string Filename { get; }

        void LoadIfExists();
        void Reload();
        bool Save();
    }

    public interface IConfigurationSerializer<TModel> where TModel : class, new()
    {
        TModel Get();
        bool Save(TModel model);
        bool Save(TModel model, Stream destination);
    }

    internal class ConfigurationSerializer<TModel, TSerializer> : IConfigurationSerializer, IConfigurationSerializer<TModel>
        where TModel : class, new()
        where TSerializer : XmlSerializer
    {
        public string Filename { get; private set; }

        private TModel _instance;
        private object _instanceLock;

        public ConfigurationSerializer(string filename)
        {
            this.Filename = filename;
            this._instanceLock = new object();
        }

        public void LoadIfExists()
        {
            if (File.Exists(Path.Combine(Installation.Properties.ConfigurationDirectory, Filename)))
                CreateInstance();
        }

        public TModel Get()
        {
            return CreateInstance();
        }

        private TModel CreateInstance()
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    // This extra check is needed because there is a (theoretical?) race condition, where two threads evaluate the if(_instance == null) at
                    // the same time. The second thread which gets the lock doesn't have to load the settings again, as it's already been done by the first
                    // thread between the check and getting the lock.
                    if (_instance == null)
                        _instance = ReadFromDisk();
                }
            }

            return _instance;
        }

        private TModel ReadFromDisk()
        {
            string path = Path.Combine(Installation.Properties.ConfigurationDirectory, Filename);

            // If the configuration file doesn't exist, copy the default configuration file
            if (!File.Exists(path))
            {
                // copy from default location
                File.Copy(Path.Combine(Installation.Properties.DefaultConfigurationDirectory, Filename), path);

                // allow everyone to write to the config
                var acl = File.GetAccessControl(path);
                SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                FileSystemAccessRule rule = new FileSystemAccessRule(everyone, FileSystemRights.FullControl, AccessControlType.Allow);
                acl.AddAccessRule(rule);
                File.SetAccessControl(path, acl);
            }

            try
            {
                return UnsafeParse(path);
            }
            catch (Exception ex)
            {
                return HandleMalformedFile(ex, path);
            }
        }

        protected TModel UnsafeParse(string path)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var deserializer = Activator.CreateInstance<TSerializer>();
                return (TModel)deserializer.Deserialize(stream);
            }
        }

        protected virtual TModel HandleMalformedFile(Exception problem, string configPath)
        {
            try
            {
                // create backup
                try
                {
                    string backupPath = Path.Combine(Installation.Properties.ConfigurationBackupDirectory, Filename);
                    if (!Directory.Exists(Path.GetDirectoryName(backupPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    File.Copy(configPath, backupPath, true);
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Failed to backup config file {0}", Filename), ex);
                }

                // cleanup the malformed file
                return CleanupMalformedFile(problem, configPath);
            }
            catch (Exception ex)
            {
                // Whatever... you probably fucked up badly with development versions to let this happen, so sort out the mess yourself.
                Log.Error(String.Format("Giving up on malformed configuration file {0}; sort out the mess yourself", Filename), ex);
                return new TModel();
            }
        }

        protected virtual TModel CleanupMalformedFile(Exception problem, string configPath)
        {
            Log.Warn("Failed to deserialize {0}, replacing with default file (error: {1})", Filename, problem.Message);
            File.Copy(Path.Combine(Installation.Properties.DefaultConfigurationDirectory, Filename), configPath, true);
            return UnsafeParse(configPath);
        }

        public bool Save(TModel model, Stream destination)
        {
            try
            {
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.CloseOutput = true;
                writerSettings.Indent = true;
                writerSettings.OmitXmlDeclaration = false;

                using (XmlWriter writer = XmlWriter.Create(destination, writerSettings))
                {
                    var serializer = Activator.CreateInstance<TSerializer>();
                    serializer.Serialize(writer, model);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to save settings for {0}", Filename), ex);
                return false;
            }
        }

        public bool Save(TModel model)
        {
            string path = Path.Combine(Installation.Properties.ConfigurationDirectory, Filename);
            using (var stream = new FileStream(path, FileMode.OpenOrCreate | FileMode.Truncate, FileAccess.Write, FileShare.Read))
            {
                return Save(model, stream);
            }
        }

        public bool Save()
        {
            lock (_instanceLock)
            {
                // If the settings haven't been loaded, they can't have been changed, so don't do anything.
                if (_instance == null)
                    return true;

                return Save(_instance);
            }
        }

        public void Reload()
        {
            // If the lock isn't available, that means that the configuration file is already being loaded and we are probably
            // triggered because we wrote to it ourself (either for a Save() call or overwriting it with the default settings).
            if (Monitor.TryEnter(_instanceLock))
            {
                _instance = ReadFromDisk();
                Monitor.Exit(_instanceLock);
            }
        }
    }

    internal class ConfigurationSerializer<TModel, TSerializer, TUpgrader> : ConfigurationSerializer<TModel, TSerializer>
        where TModel : class, new()
        where TSerializer : XmlSerializer
        where TUpgrader : ConfigUpgrader<TModel>, new()
    {
        private string upgradeFilename;

        public ConfigurationSerializer(string filename, string upgradeFilename)
            : base (filename)
        {
            this.upgradeFilename = upgradeFilename;
        }

        public ConfigurationSerializer(string filename)
            : this(filename, null)
        {
        }

        protected override TModel CleanupMalformedFile(Exception problem, string configPath)
        {
            var upgrader = new TUpgrader();
            upgrader.OldPath = upgradeFilename == null ? configPath : Path.Combine(Installation.Properties.ConfigurationDirectory, upgradeFilename);
            upgrader.DefaultPath = Path.Combine(Installation.Properties.DefaultConfigurationDirectory, Filename);

            if (upgrader.CanUpgrade())
            {
                Log.Info("Failed to deserialize {0}, upgrading config file now", Filename);
                TModel model = upgrader.PerformUpgrade();
                Save(model);
                return model;
            }
            else
            {
                Log.Warn("Failed to deserialize {0}, replacing with default file (error: {1})", Filename, problem.Message);
                File.Copy(Path.Combine(Installation.Properties.DefaultConfigurationDirectory, Filename), configPath, true);
                return UnsafeParse(configPath);
            }
        }
    }
}
