#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service.Util;

namespace MPExtended.Libraries.Service.Config
{
    public interface IConfigurationSerializer
    {
        ConfigurationFile ConfigFile { get; }
        string Filename { get; }

        void LoadIfExists();
        void Reload();
        bool Save();
    }

    public interface IConfigurationSerializer<TModel> : IConfigurationSerializer where TModel : class, new()
    {
        TModel Get();
        bool Save(TModel model);
        bool Save(TModel model, Stream destination);
    }

    internal class ConfigurationSerializer<TModel, TSerializer> : IConfigurationSerializer, IConfigurationSerializer<TModel>
        where TModel : class, new()
        where TSerializer : XmlSerializer
    {
        public ConfigurationFile ConfigFile { get; private set; }
        public string Filename { get; private set; }

        private TModel _instance;
        private object _instanceLock;

        public ConfigurationSerializer(ConfigurationFile file, string filename)
        {
            this.ConfigFile = file;
            this.Filename = filename;
            this._instanceLock = new object();
        }

        public virtual void LoadIfExists()
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
                CreateNonExistingFile(path);
            }

            try
            {
                return UnsafeParse(path);
            }
            catch (Exception ex)
            {
                Log.Trace("Configuration: Failed to read configuration file from disk, going to HandleMalformedFile()");
                return HandleMalformedFile(ex, path);
            }
        }

        protected virtual void CreateNonExistingFile(string path)
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

        protected TModel UnsafeParse(string path)
        {
            Log.Trace("Configuration: Reading configuration file {0} from {1}", Filename, path);
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return UnsafeParse(stream);
            }
        }

        protected TModel UnsafeParse(Stream stream)
        {
            var deserializer = Activator.CreateInstance<TSerializer>();
            return (TModel)deserializer.Deserialize(stream);
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
                    Log.Error(String.Format("Configuration: Failed to backup config file {0}", Filename), ex);
                }

                // cleanup the malformed file
                return CleanupMalformedFile(problem, configPath);
            }
            catch (Exception ex)
            {
                // Whatever... you probably fucked up badly with development versions to let this happen, so sort out the mess yourself.
                Log.Error(String.Format("Configuration: Giving up on malformed configuration file {0}; sort out the mess yourself", Filename), ex);
                return new TModel();
            }
        }

        protected virtual TModel CleanupMalformedFile(Exception problem, string configPath)
        {
            Log.Warn("Configuration: Failed to deserialize {0}, replacing with default file (error: {1})", Filename, problem.Message);
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
                Log.Error(String.Format("Configuration: Failed to save settings for {0}", Filename), ex);
                return false;
            }
        }

        public bool Save(TModel model)
        {
            Log.Trace("Configuration: Writing new version of {0}", Filename);
            string path = Path.Combine(Installation.Properties.ConfigurationDirectory, Filename);
            using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                stream.SetLength(0);
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
                string path = Path.Combine(Installation.Properties.ConfigurationDirectory, Filename);
                var stream = FileUtil.TryOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read, 5000);
                if (stream != null)
                {
                    Log.Trace("Configuration: Reading configuration file {0} from {1}.", Filename, path);
                    try
                    {
                        _instance = UnsafeParse(stream);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Configuration: Keep using old configuration because new one is invalid.", ex);
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
                else
                {
                    Log.Warn("Configuration: Failed to open configuration file {0} from {1}.", Filename, path);
                }

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

        public ConfigurationSerializer(ConfigurationFile file, string filename, string upgradeFilename)
            : base(file, filename)
        {
            this.upgradeFilename = upgradeFilename;
        }

        public ConfigurationSerializer(ConfigurationFile file, string filename)
            : this(file, filename, null)
        {
        }

        public override void LoadIfExists()
        {
            string path = Path.Combine(Installation.Properties.ConfigurationDirectory, Filename);
            if (File.Exists(path) || upgradeFilename == null)
            {
                base.LoadIfExists();
                return;
            }

            var upgrader = new TUpgrader();
            upgrader.OldPath = Path.Combine(Installation.Properties.ConfigurationDirectory, upgradeFilename);
            upgrader.DefaultPath = Path.Combine(Installation.Properties.DefaultConfigurationDirectory, Filename);

            if (upgrader.CanUpgrade())
            {
                Log.Info("Config file {0} doesn't exist, creating from old config file {1}", Filename, upgradeFilename);
                TModel model = upgrader.PerformUpgrade();
                Save(model);
            }

            base.LoadIfExists();
        }

        protected override void CreateNonExistingFile(string path)
        {
            if (upgradeFilename == null)
            {
                base.CreateNonExistingFile(path);
                return;
            }

            var upgrader = new TUpgrader();
            upgrader.OldPath = Path.Combine(Installation.Properties.ConfigurationDirectory, upgradeFilename);
            upgrader.DefaultPath = Path.Combine(Installation.Properties.DefaultConfigurationDirectory, Filename);

            if (upgrader.CanUpgrade())
            {
                Log.Info("Config file {0} doesn't exist, creating from old config file {1}", Filename, upgradeFilename);
                TModel model = upgrader.PerformUpgrade();
                Save(model);
            }
            else
            {
                base.CreateNonExistingFile(path);
            }
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
