#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Units;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Pipeline
    {
        private Dictionary<int, IProcessingUnit> dataUnits = new Dictionary<int, IProcessingUnit>();
        private Dictionary<int, ILogProcessingUnit> logUnits = new Dictionary<int, ILogProcessingUnit>();
        public bool IsAssembled { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }

        public Pipeline()
        {
            IsAssembled = false;
            IsStarted = false;
            IsStopped = false;
        }

        public void AddDataUnit(IProcessingUnit process, int position)
        {
            dataUnits[position] = process;
            dataUnits[position].IsInputStreamConnected = false;
            dataUnits[position].IsDataStreamConnected = false;
            dataUnits[position].IsLogStreamConnected = false;
        }

        public void AddLogUnit(ILogProcessingUnit process, int position)
        {
            logUnits[position] = process;
        }

        public IProcessingUnit GetDataUnit(int position)
        {
            if (!dataUnits.ContainsKey(position))
                return null;
            return dataUnits[position];
        }

        public ILogProcessingUnit GetLogUnit(int position)
        {
            if (!logUnits.ContainsKey(position))
                return null;
            return logUnits[position];
        }

        public bool Assemble()
        {
            IsAssembled = true;
            Dictionary<int, int> dataConnections = new Dictionary<int, int>();
            Dictionary<int, int> logConnections = new Dictionary<int, int>();

            int lastKey = -1;
            foreach (int i in dataUnits.Keys.OrderBy(k => k))
            {
                if (dataUnits.ContainsKey(lastKey))
                {
                    dataConnections[lastKey] = i;
                    dataUnits[i].IsInputStreamConnected = true;
                    dataUnits[lastKey].IsDataStreamConnected = true;
                }

                lastKey = i;
            }

            foreach (int i in logUnits.Keys.OrderBy(k => k))
            {
                int nr = dataUnits.Keys.Where(k => k < i).DefaultIfEmpty(-1).Max();
                if (dataUnits.ContainsKey(nr))
                {
                    logConnections[i] = nr;
                    dataUnits[nr].IsLogStreamConnected = true;
                }
            }

            // connect the output of the last one as that's usually captured by the calling app (because else the whole pipeline is just useless)
            dataUnits[dataUnits.Keys.Max()].IsDataStreamConnected = true;

            // dump out the pipeline for debugging
            Log.Info("Assembling following pipeline:");
            foreach (int i in dataUnits.Keys.OrderBy(k => k))
                Log.Info("   data {0}: {1} (input {2}, data {3}, log {4})", i, dataUnits[i].ToString(), dataUnits[i].IsInputStreamConnected, dataUnits[i].IsDataStreamConnected, dataUnits[i].IsLogStreamConnected);
            foreach (KeyValuePair<int, int> conn in dataConnections)
                Log.Info("   dataconn {0} -> {1}", conn.Key, conn.Value);
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                Log.Info("   log  {0}: {1}", i, logUnits[i].ToString());
            foreach (KeyValuePair<int, int> conn in logConnections)
                Log.Info("   logconn {0} -> {1}", conn.Value, conn.Key);

            foreach (int i in dataUnits.Keys.OrderBy(k => k))
            {
                if (!dataUnits[i].Setup())
                {
                    // it failed, stop and break out
                    Log.Error("Setting up data unit {0} failed", i);
                    Stop(true);
                    return false;
                }
                else
                {
                    Log.Debug("Setup data unit {0}", i);
                }

                if (dataConnections.ContainsKey(i))
                    dataUnits[dataConnections[i]].InputStream = dataUnits[i].DataOutputStream;
            }

            foreach (int i in logUnits.Keys.OrderBy(k => k))
            {
                if (logConnections.ContainsKey(i))
                    logUnits[i].InputStream = dataUnits[logConnections[i]].LogOutputStream;
                logUnits[i].Setup();
                Log.Debug("Setup log unit {0}", i);
            }

            Log.Info("Pipeline assembled");
            return true;
        }

        public bool Start()
        {
            if (!IsAssembled)
                Assemble();
            IsStarted = true;

            foreach (int i in dataUnits.Keys.OrderBy(k => k))
            {
                Log.Info("Starting data unit {0}", i);
                if (!dataUnits[i].Start())
                {
                    Log.Error("Starting data unit {0} failed", i);
                    Stop(true);
                    return false;
                }
            }

            Log.Info("All data units started!");
            foreach (int i in logUnits.Keys.OrderBy(k => k))
            {
                logUnits[i].Start();
            }

            return true;
        }

        public bool Stop(bool force)
        {
            if (IsStopped)
                return true;
            if (!IsStarted && !force)
                Start();

            foreach (int i in dataUnits.Keys.OrderBy(k => k))
            {
                Log.Info("Stopping data unit {0}", i);
                dataUnits[i].Stop();
            }

            foreach (int i in logUnits.Keys.OrderBy(k => k))
            {
                logUnits[i].Stop();
            }

            Log.Debug("Pipeline stopped");
            IsStopped = true;
            return true;
        }

        public bool Stop()
        {
            return Stop(false);
        }

        public Stream GetFinalStream()
        {
            if (!IsStarted)
                return null;

            return dataUnits[dataUnits.Keys.Max()].DataOutputStream;
        }

        public bool RunBlocking()
        {
            if (!IsStarted)
                Start();

            foreach (int i in dataUnits.Keys.OrderBy(k => k))
            {
                if (dataUnits[i] is IBlockingProcessingUnit)
                {
                    ((IBlockingProcessingUnit)dataUnits[i]).RunBlocking();
                    return true;
                }
            }

            return false;
        }
    }
}
