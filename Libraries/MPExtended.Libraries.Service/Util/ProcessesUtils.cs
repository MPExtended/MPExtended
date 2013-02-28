using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MPExtended.Libraries.Service.Util
{
    public class ProcessesUtils
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
                    if (p.ProcessName.Contains(name))
                    {
                        //process is found
                        return p;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //process not found -> return null
            return null;
        }
    }
}
