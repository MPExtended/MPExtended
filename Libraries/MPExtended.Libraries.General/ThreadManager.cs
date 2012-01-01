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

namespace MPExtended.Libraries.General
{
    public static class ThreadManager
    {
        private static List<Thread> threads = new List<Thread>();

        public static void Add(Thread t)
        {
            threads.Add(t);
        }

        public static void Remove(Thread t)
        {
            threads.Remove(t);
        }

        public static Thread Start(string name, ThreadStart method) 
        {
            Thread t = new Thread(method);
            t.Name = name;
            t.Start();
            threads.Add(t);
            return t;
        }

        public static Thread Start(string name, ParameterizedThreadStart method, object parameter)
        {
            Thread t = new Thread(method);
            t.Name = name;
            t.Start(parameter);
            threads.Add(t);
            return t;
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
