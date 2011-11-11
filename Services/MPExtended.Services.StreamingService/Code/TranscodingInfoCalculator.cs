#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Libraries.General;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class TranscodingInfoCalculator
    {
        private const int FPS_SAMPLING_RATE = 5; // in seconds

        public int SamplingRate { get; set; } // in milliseconds
        public int FPS { get; set; }
        public int StartPosition { get; set; } // in milliseconds

        private int milliseconds;
        private int counter;
        private int lastCountMilliseconds;
        private int calculatedFPS;

        private long durationMilliseconds;
        private bool loggedUnknownDuration = false;

        /// <param name="startPosition">Start position in milliseconds</param>
        /// <param name="fps">The number of frames that are encoded per second</param>
        public TranscodingInfoCalculator(int startPosition, int fps, int samplingRate)
        {
            this.StartPosition = startPosition;
            this.FPS = fps;
            this.SamplingRate = samplingRate;
        }

        public TranscodingInfoCalculator(int startPosition, int fps, int samplingRate, long durationMilliseconds)
            : this(startPosition, fps, samplingRate)
        {
            this.durationMilliseconds = durationMilliseconds;
        }

        /// <param name="newTime">New time there's transcoded to in milliseconds</param>
        public void NewTime(int newTime)
        {
            int fpsCount = FPS_SAMPLING_RATE * 1000 / SamplingRate;

            milliseconds = newTime;
            if (counter++ % fpsCount == 0)
            {
                calculatedFPS = ((newTime - lastCountMilliseconds) / (1000 / FPS)) / FPS_SAMPLING_RATE;
                lastCountMilliseconds = newTime;
            }
        }

        public void NewPercentage(double percentage)
        {
            if (this.durationMilliseconds == 0)
            {
                if (!loggedUnknownDuration)
                {
                    Log.Warn("Called NewPercentage({0}) but duration is unknown!", percentage);
                }
                loggedUnknownDuration = true;
                return;
            }

            NewTime((int)Math.Round(percentage * this.durationMilliseconds));
        }

        public void SetStats(Reference<WebTranscodingInfo> output)
        {
            lock (output.Value)
            {
                output.Value.EncodingFPS = calculatedFPS;
                output.Value.EncodedFrames = (milliseconds - StartPosition) / (1000 / FPS);
                output.Value.CurrentTime = milliseconds;
            }
        }
    }
}
