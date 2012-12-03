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
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service;

namespace MPExtended.PlugIns.MAS.MPShares
{
    internal static class ConfigurationChangeListener
    {
        public delegate void ConfigurationChangedEventHandler();
        public static event ConfigurationChangedEventHandler ConfigurationChanged;

        private static FileSystemWatcher watcher;

        public static void Enable()
        {
            if (watcher != null)
                return;

            string path = Mediaportal.GetConfigFilePath();
            watcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path));
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += delegate(object sender, FileSystemEventArgs e)
            {
                try
                {
                    if (ConfigurationChanged != null)
                        ConfigurationChanged();
                }
                catch (Exception ex)
                {
                    Log.Warn("Failed to reload MPShares configuration", ex);
                }
            };
            watcher.EnableRaisingEvents = true;
        }
    }
}
