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
using System.Threading;

namespace MPExtended.Libraries.Service.Util
{
    public static class FileUtil
    {
        private const int TRY_OPEN_DELAY_TIME = 50;

        public static FileStream TryOpen(string path, FileMode mode, FileAccess access, FileShare share, int maxDelay)
        {
            int tries = 0;
            do
            {
                try
                {
                    return File.Open(path, mode, access, share);
                }
                catch (IOException)
                {
                    // Ignore it, and just retry after the delay
                }

                if (++tries * TRY_OPEN_DELAY_TIME <= maxDelay)
                    Thread.Sleep(TRY_OPEN_DELAY_TIME);
            } while (tries * TRY_OPEN_DELAY_TIME <= maxDelay);

            return null;
        }
    }
}
