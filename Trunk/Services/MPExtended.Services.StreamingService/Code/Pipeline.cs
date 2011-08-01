﻿#region Copyright
/* 
 *  Copyright (C) 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.StreamingService.Util;

namespace MPExtended.Services.StreamingService.Code {
    internal class Pipeline {
        private Dictionary<int, IProcessingUnit> dataUnits = new Dictionary<int, IProcessingUnit>();
        private Dictionary<int, ILogProcessingUnit> logUnits = new Dictionary<int, ILogProcessingUnit>();
        public bool IsAssembled { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }

        public Pipeline() {
            IsAssembled = false;
            IsStarted = false;
            IsStopped = false;
        }

        public void AddDataUnit(IProcessingUnit process, int position) {
            dataUnits[position] = process;
            dataUnits[position].IsInputStreamConnected = false;
            dataUnits[position].IsDataStreamConnected = false;
            dataUnits[position].IsLogStreamConnected = false;
        }

        public void AddLogUnit(ILogProcessingUnit process, int position) {
            logUnits[position] = process;
        }

        public IProcessingUnit GetDataUnit(int position) {
            if (!dataUnits.ContainsKey(position))
                throw new ArgumentException();
            return dataUnits[position];
        }

        public ILogProcessingUnit GetLogUnit(int position) {
            if (!logUnits.ContainsKey(position))
                throw new ArgumentException();
            return logUnits[position];
        }

        public bool Assemble() {
            IsAssembled = true;
            Dictionary<int, int> dataConnections = new Dictionary<int,int>();
            Dictionary<int, int> logConnections = new Dictionary<int,int>();

            int lastKey = -1;
            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                if (dataUnits.ContainsKey(lastKey)) {
                    dataConnections[lastKey] = i;
                    dataUnits[i].IsInputStreamConnected = true;
                    dataUnits[lastKey].IsDataStreamConnected = true;
                }

                lastKey = i;
            }

            foreach (int i in logUnits.Keys.OrderBy(k => k)) {
                int nr = dataUnits.Keys.Where(k => k < i).DefaultIfEmpty(-1).Max();
                if (dataUnits.ContainsKey(nr)) {
                    logConnections[i] = nr;
                    dataUnits[nr].IsLogStreamConnected = true;
                }
            }

            // connect the output of the last one as that's usually captured by the calling app (because else the whole pipeline is just useless)
            dataUnits[dataUnits.Keys.Max()].IsDataStreamConnected = true;

            // dump out the pipeline for debugging
            Log.Write("Assembling following pipeline:");
            foreach (int i in dataUnits.Keys.OrderBy(k => k))
                Log.Write("   data {0}: {1} (input {2}, data {3}, log {4})", i, dataUnits[i].ToString(), dataUnits[i].IsInputStreamConnected, dataUnits[i].IsDataStreamConnected, dataUnits[i].IsLogStreamConnected);
            foreach (KeyValuePair<int, int> conn in dataConnections)
                Log.Write("   dataconn {0} -> {1}", conn.Key, conn.Value);
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                Log.Write("   log  {0}: {1}", i, logUnits[i].ToString());
            foreach (KeyValuePair<int, int> conn in logConnections)
                Log.Write("   logconn {0} -> {1}", conn.Value, conn.Key);

            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                if (!dataUnits[i].Setup()) {
                    // it failed, stop and break out
                    Log.Error("Setup of data unit {0} failed", i);
                    Stop();
                    return false;
                }   
                if(dataConnections.ContainsKey(i))
                    dataUnits[dataConnections[i]].InputStream = dataUnits[i].DataOutputStream;
            }
            foreach (int i in logUnits.Keys.OrderBy(k => k)) {
                logUnits[i].Setup();
                if (logConnections.ContainsKey(i))
                    logUnits[i].InputStream = dataUnits[logConnections[i]].LogOutputStream;
            }

            Log.Write("Pipeline assembled");
            return true;
        }

        public bool Start() {
            if (!IsAssembled)
                Assemble();
            IsStarted = true;

            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                Log.Write("Starting data unit {0}", i);
                if (!dataUnits[i].Start()) {
                    Log.Error("Starting data unit {0} failed", i);
                    Stop();
                    return false;
                }
            }
            Log.Write("All data units started!");
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                logUnits[i].Start();

            return true;
        }

        public bool Stop() {
            if (IsStopped)
                return true;
            if (!IsStarted)
                Start();

            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                Log.Write("Stopping data unit {0}", i);
                dataUnits[i].Stop();
            }
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                logUnits[i].Stop();

            Log.Debug("Pipeline stopped");
            IsStopped = true;
            return true;
        }

        public Stream GetFinalStream() {
            if (!IsStarted)
                return null;

            return dataUnits[dataUnits.Keys.Max()].DataOutputStream;
        }

        public bool RunBlocking() {
            if (!IsStarted)
                Start();

            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                if (dataUnits[i] is IBlockingProcessingUnit) {
                    ((IBlockingProcessingUnit)dataUnits[i]).RunBlocking();
                    return true;
                }
            }

            return false;
        }
    }
}