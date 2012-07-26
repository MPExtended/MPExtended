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
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MPExtended.Libraries.Service.Config
{
    internal class ConfigurationSerializer<TModel, TSerializer> where TModel : new()
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

        public TModel Get()
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    // This extra check is needed because there is a (theoretical?) race condition, where two threads evaluate the if(_instance == null) at
                    // the same time. The second thread which enters the lock statement doesn't have to load the settings again, as it's already been done by
                    // the first thread between the check and entering the lock statement.
                    if (_instance == null)
                        _instance = Load();
                }
            }

            return _instance;
        }

        private TModel Load()
        {
            string path = Configuration.GetPath(Filename);
            try
            {
                return UnsafeParse(path);
            }
            catch (Exception ex)
            {
                return HandleMalformedFile(ex, path);
            }
        }

        private TModel UnsafeParse(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var deserializer = Activator.CreateInstance<TSerializer>();
                return (TModel)deserializer.Deserialize(stream);
            }
        }

        private TModel HandleMalformedFile(Exception problem, string configPath)
        {
            try
            {
                // TODO: check whether upgrading the config file is possible
                bool canUpgrade = false;

                // create backup
                try
                {
                    string backupPath = Path.Combine(Installation.GetConfigurationDirectory(), "ConfigBackup", Filename);
                    if (!Directory.Exists(Path.GetDirectoryName(backupPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                    File.Copy(configPath, backupPath, true);
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Failed to backup config file {0}", Filename), ex);
                }

                if (canUpgrade)
                {
                    Log.Info("Failed to deserialize {0}, upgrading config file now", Filename);
                    // TODO: actually get a model of the upgraded config file in the line below
                    TModel model = new TModel();
                    Save(model);
                    return model;
                }
                else
                {
                    Log.Warn("Failed to deserialize {0}, replacing with default file (error: {1})", Filename, problem.Message);
                    File.Copy(Configuration.GetDefaultPath(Filename), configPath, true);
                    return UnsafeParse(configPath);
                }
            }
            catch (Exception ex)
            {
                // Whatever... you probably fucked up badly with development versions to let this happen, so sort out the mess yourself.
                Log.Error(String.Format("Giving up on malformed configuration file {0}; sort out the mess yourself", Filename), ex);
                return new TModel();
            }
        }

        public bool Save(TModel model)
        {
            try
            {   
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.CloseOutput = true;
                writerSettings.Indent = true;
                writerSettings.OmitXmlDeclaration = false;
                using (XmlWriter writer = XmlWriter.Create(Configuration.GetPath(Filename), writerSettings))
                {
                    var serializer = Activator.CreateInstance<TSerializer>();
                    serializer.Serialize(writer, model);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to save settings to file {0}", Filename), ex);
                return false;
            }
        }

        public bool Save()
        {
            // If the settings haven't been loaded, they can't have been changed, so don't do anything.
            if (_instance == null)
                return true;
            return Save(_instance);
        }
    }
}
