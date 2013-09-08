#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Diagnostics;

namespace MPExtended.Libraries.Service.Util
{
    public class ProcessUtils
    {
        /// <summary>
        /// Check if one of the given processes is running
        /// </summary>
        /// <param name="conflicting">List of process names to check for</param>
        /// <returns>true if the process is found, false otherwise</returns>
        public static bool IsProcessRunning(String[] conflicting)
        {
            Process[] runningProcesses = Process.GetProcesses();

            foreach (string p in conflicting)
            {
                Process match = GetProcess(p, runningProcesses);

                if (match != null && match.MainWindowTitle != null)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Get running process by name
        /// </summary>
        /// <param name="name">process name</param>
        /// <returns>Process object if one is found, null otherwises</returns>
        public static Process GetProcess(string name)
        {
            return GetProcess(name, Process.GetProcesses());
        }

        /// <summary>
        /// Get process out of a given list of processes
        /// </summary>
        /// <param name="name">process name</param>
        /// <param name="running">list of processes where we search in</param>
        /// <returns>Process object if one is found, null otherwises</returns>
        public static Process GetProcess(string name, Process[] running)
        {
            try
            {
                foreach (Process p in running)
                {
                    if (p.ProcessName.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //process is found
                        return p;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Error getting process", ex);
            }
            //process not found -> return null
            return null;
        }
    }
}
