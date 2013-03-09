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
using System.Linq;
using System.Text;
using System.IO;
using MPExtended.Libraries.Service;

namespace MPExtended.Libraries.Service.Hosting
{
    public static class LogRotation
    {
        public static void Rotate()
        {
            string directory = Installation.GetLogDirectory();
            var files = Directory.GetFiles(directory).Where(x => Path.GetExtension(x) == ".log");
            foreach (var file in files)
            {
                FileInfo info = new FileInfo(file);
                if(info.Length > 1024 * 1024)
                {
                    string backup = Path.ChangeExtension(file, ".bak");
                    if (File.Exists(backup))
                    {
                        File.Delete(backup);
                    }

                    try
                    {
                        File.Move(file, backup);
                    }
                    catch (Exception)
                    {
                        // damnit!
                    }
                }
            }
        }
    }
}
