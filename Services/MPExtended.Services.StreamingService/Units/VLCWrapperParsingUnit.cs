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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MPExtended.Libraries.General;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Units
{
    internal class VLCWrapperParsingUnit : ILogProcessingUnit
    {
        public Stream InputStream { get; set; }
        private Reference<WebTranscodingInfo> data;
        private WebMediaInfo info;
        private Thread processThread;
        private bool vlcIsStarted;
        private int position;

        public VLCWrapperParsingUnit(Reference<WebTranscodingInfo> save, WebMediaInfo info, int position) 
        {
            data = save;
            this.info = info;
            this.position = position;
        }

        public bool Setup() 
        {
            data.Value.Failed = false;
            vlcIsStarted = false;
            processThread = ThreadManager.Start("VLCLogParsing", delegate()
            {
                ParseOutputStream(InputStream, data, position, false);
            });
            return true;
        }

        public bool Start() 
        {
            while (!vlcIsStarted)
                Thread.Sleep(100);
            return true;
        }

        public bool Stop() 
        {
            processThread.Abort();
            return true;
        }

        private void ParseOutputStream(Stream stdoutStream, Reference<WebTranscodingInfo> data, int startPosition, bool logProgress)
        {
            StreamReader reader = new StreamReader(stdoutStream);
            TranscodingInfoCalculator calculator = new TranscodingInfoCalculator(position, 25, 500, info.Duration); //VLCWrapper prints twice a second

            bool aborted = false;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    Log.Trace("VLCWrapperParsing: read line {0}", line);

                    // just for debugging of the wrapper tool
                    if(line.StartsWith("A") || line == "S started" || line == "S null")
                        continue;

                    // propagate start event to Start() method
                    if (line == "S playing")
                    {
                        vlcIsStarted = true;
                        continue;
                    }

                    // events
                    if (line == "S error")
                    {
                        vlcIsStarted = true;
                        data.Value.Finished = true;
                        data.Value.Failed = true;
                        break;
                    }

                    if (line == "S finished")
                    {
                        data.Value.Failed = false;
                        data.Value.Finished = true;
                        continue;
                    }

                    // the actual progress parsing
                    if(line.StartsWith("P")) 
                    {
                        // the format is 'P [time in microseconds] [percentage of file]'. Sadly we can't use the way more detailed time in microseconds
                        // because the VLC guys decided it would be a good idea to convert it to a 32-bit integer before returning it, as opposed to
                        // returning libvlc_time_t or a int64_t. And since it's in microseconds it gets big very fast, so it's absolutely useless.
                        double percentage = Double.Parse(line.Substring(line.IndexOf(",") + 2), CultureInfo.InvariantCulture);
                        calculator.NewPercentage(percentage);
                        calculator.SetStats(data);
                        continue;
                    }

                    Log.Warn("VLCWrapperParsing: encountered unknown line {0}", line);
                }
                catch (ThreadAbortException)
                {
                    aborted = true;
                    break;
                }
                catch (Exception e)
                {
                    Log.Error("Failure during parsing of VLC output", e);
                }
            }

            data.Value.Failed = aborted;
            data.Value.Finished = true;
            reader.Close();
            return;
        }
    }
}
