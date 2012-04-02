#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Threading;

namespace MPExtended.Libraries.Service
{
    public static class ThreadManager
    {
        private class ThreadStartArguments
        {
            public string Name { get; set; }
            public string StackTrace { get; set; }

            public bool HasParameter { get; set; }
            public object Parameter { get; set; }
            public ThreadStart StartMethod { get; set; }
            public ParameterizedThreadStart ParameterStartMethod { get; set; }
        }

        private static List<Thread> threads = new List<Thread>();

        public static void Remove(Thread t)
        {
            threads.Remove(t);
        }

        [Obsolete("Please use the modern replacements of low-level threading, such as the Task Parallel Library, Timers or BackgroundWorkers.")]
        public static Thread Start(string name, ThreadStart method) 
        {
            return Start(new ThreadStartArguments()
            {
                Name = name,
                StackTrace = Environment.StackTrace,
                HasParameter = false,
                StartMethod = method
            });
        }

        [Obsolete("Please use the modern replacements of low-level threading, such as the Task Parallel Library, Timers or BackgroundWorkers.")]
        public static Thread Start(string name, ParameterizedThreadStart method, object parameter)
        {
            return Start(new ThreadStartArguments()
            {
                Name = name,
                StackTrace = Environment.StackTrace,
                HasParameter = true,
                Parameter = parameter,
                ParameterStartMethod = method
            });
        }

        private static Thread Start(ThreadStartArguments arguments)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(delegate(object threadArguments) {
                ThreadStartArguments tsa = threadArguments as ThreadStartArguments;
                try
                {
                    if (tsa.HasParameter)
                    {
                        tsa.ParameterStartMethod.Invoke(tsa.Parameter);
                    }
                    else
                    {
                        tsa.StartMethod.Invoke();
                    }
                }
                catch (ThreadAbortException)
                {
                    // we don't care about these
                }
                catch (Exception ex)
                {
                    string message = String.Format("Thread {0} aborting due to uncaught exception", tsa.Name);
                    Log.Error(message, ex);
                    Log.Info("Thread was started at:\r\n{0}", tsa.StackTrace);
                }
            }));

            thread.Name = arguments.Name;
            thread.Start(arguments);
            threads.Add(thread);
            return thread;
        }

        public static void Abort(string name)
        {
            foreach(var thread in threads.Where(x => x.Name == name).ToList()) 
            {
                Abort(thread);
            }
        }

        public static void Abort(Thread thread)
        {
            try
            {
                thread.Abort();
                threads.Remove(thread);
            }
            catch (Exception ex)
            {
                Log.Info("Failed to abort thread", ex);
            }
        }

        public static void AbortAll()
        {
            foreach (Thread t in threads.ToList())
            {
                Abort(t);
            }
        }
    }
}
