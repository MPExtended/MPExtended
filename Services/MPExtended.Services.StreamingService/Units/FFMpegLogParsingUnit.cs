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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MPExtended.Libraries.General;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Units 
{
    internal class FFMpegLogParsingUnit : ILogProcessingUnit 
    {
        public Stream InputStream { get; set; }
        public bool LogMessages { get; set; }
        public bool LogProgress { get; set; }
        private Reference<WebTranscodingInfo> data;
        private Thread processThread;
        private int startPosition;

        public FFMpegLogParsingUnit(Reference<WebTranscodingInfo> save, int startPosition) 
        {
            data = save;
            this.startPosition = startPosition;
        }

        public bool Setup() 
        {
            // this might be better placed in the Start() method, but EncoderUnit.Start() depends on this
            data.Value.Supported = true;
            data.Value.Failed = false;
            processThread = ThreadManager.Start("FFMpegLogParsing", delegate()
            {
                ParseOutputStream(InputStream, data, startPosition, LogMessages, LogProgress);
            });
            return true;
        }

        public bool Start() 
        {
            return true;
        }

        public bool Stop() 
        {
            ThreadManager.Abort(processThread);
            return true;
        }

        private static void ParseOutputStream(Stream outputStream, Reference<WebTranscodingInfo> saveData, int startPosition, bool logMessages, bool logProgress)
        {
            StreamReader reader = new StreamReader(outputStream);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    bool canBeErrorLine = true;

                    if (line.StartsWith("frame="))
                    {
                        // format of an output line (yes, we're doomed as soon as ffmpeg changes it output):
                        // frame=  923 fps=256 q=31.0 size=    2712kB time=00:05:22.56 bitrate= 601.8kbits/s
                        Match match = Regex.Match(line, @"frame=([ 0-9]*) fps=([ 0-9]*) q=[^ ]* L?size=([ 0-9]*)kB time=([0-9]{2}):([0-9]{2}):([0-9]{2})\.[0-9]{2} bitrate=([ .0-9]*)kbits/s", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            canBeErrorLine = false;
                            lock (saveData)
                            {
                                saveData.Value.TranscodedTime = (Int32.Parse(match.Groups[4].Value) * 3600 + Int32.Parse(match.Groups[5].Value) * 60 + Int32.Parse(match.Groups[6].Value)) * 1000;
                                saveData.Value.TranscodedFrames = Int32.Parse(match.Groups[1].Value);
                                saveData.Value.TranscodingPosition = startPosition + saveData.Value.TranscodedTime;
                                saveData.Value.TranscodingFPS = Int32.Parse(match.Groups[2].Value);
                                saveData.Value.OutputBitrate = (int)Math.Round(Decimal.Parse(match.Groups[7].Value, System.Globalization.CultureInfo.InvariantCulture));
                            }

                            if (!logProgress) // we don't log output
                                continue;
                        }
                    }

                    if (line.StartsWith("video:"))
                    {
                        // process the result line to see if it completed successfully (example):
                        // video:5608kB audio:781kB global headers:0kB muxing overhead 13.235302%
                        Match resultMatch = Regex.Match(line, @"video:([0-9]*)kB audio:([0-9]*)kB global headers:([0-9]*)kB muxing overhead[^%]*%", RegexOptions.IgnoreCase);
                        saveData.Value.Finished = true;
                        canBeErrorLine = false;
                    }

                    // show error messages
                    if (logMessages && canBeErrorLine)
                        Log.Trace("ffmpeg: " + line);
                }
                catch (ThreadAbortException)
                {
                    saveData.Value.Failed = true;
                    break;
                }
                catch (Exception e)
                {
                    Log.Error("Failure during parsing of ffmpeg output", e);
                }
            }

            saveData.Value.Finished = true;
            reader.Close();
            return;
        }
    }
}
