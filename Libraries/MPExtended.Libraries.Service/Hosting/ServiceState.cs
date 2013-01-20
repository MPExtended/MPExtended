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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPExtended.Libraries.Service.Hosting
{
    public class ServiceState
    {
        public delegate void ServiceStartedEventHandler();
        public static event ServiceStartedEventHandler Started;

        public delegate void ServiceStoppingEventHandler();
        public static event ServiceStoppingEventHandler Stopping;

        private static List<string> waitConditions = new List<string>();

        public static void RegisterStartupCondition(string conditionName)
        {
            lock (waitConditions)
            {
                waitConditions.Add(conditionName);
            }
        }

        public static void StartupConditionCompleted(string conditionName)
        {
            lock (waitConditions)
            {
                waitConditions.Remove(conditionName);
                if (waitConditions.Count == 0)
                {
                    Task.Factory.StartNew(delegate()
                    {
                        Log.Debug("Triggering ServiceState.Started event");
                        if (Started != null)
                        {
                            Started();
                        }
                    });
                }
            }
        }

        internal static void TriggerStoppingEvent()
        {
            Log.Trace("Triggering ServiceState.Stopping event");
            if (Stopping != null)
            {
                try
                {
                    Stopping();
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to handle Stopping event", ex);
                }
            }
        }
    }
}
