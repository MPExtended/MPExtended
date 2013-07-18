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
using System.Text;

namespace MPExtended.Libraries.Service.Logging
{
    internal class FileDestination : ILogDestination, IDisposable
    {
        public LogLevel MinimumLevel { get; private set; }
        public TextWriter Output { get; private set; }

        public string LogFormat
        {
            get { return "{0:yyyy-MM-dd HH:mm:ss.fffff} [{1,15}({2,2})] {3,5}: "; }
        }

        public FileDestination(LogLevel level, string filename)
        {
            MinimumLevel = level;

            Stream file = File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            Output = TextWriter.Synchronized(new StreamWriter(file, Encoding.UTF8, 4096));
        }

        public void Dispose()
        {
            Output.Close();
        }
    }
}
