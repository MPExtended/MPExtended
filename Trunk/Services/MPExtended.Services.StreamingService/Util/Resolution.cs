#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;

namespace MPExtended.Services.StreamingService.Util {
    internal class ResolutionInfo {
        // The original resolution of the stream
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }

        // The Pixel Aspect Ratio (PAR). Is 1:1 for square pixels, most new HD material
        public int PixelAspectX { get; set; }
        public int PixelAspectY { get; set; }
        public string PixelAspectRatio {
            get {
                return PixelAspectX + ":" + PixelAspectY;
            }
        }

        // The Display Aspect Ratio (DAR). Is 16:9 for HD material. 
        // This should be the same as the original size corrected with the PAR, the rendered image (see below)
        public int DisplayAspectX { get; set; }
        public int DisplayAspectY { get; set; }
        public string DisplayAspectRatio {
            get {
                return DisplayAspectX + ":" + DisplayAspectY;
            }
        }

        // the size of the actually rendered image. 
        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }

        // check if the DAR is the same as the aspect ratio of the rendered image. if this returns false that's a bug.
        public bool Validate() {
            double DAR = DisplayAspectX / DisplayAspectY;
            double renderAspect = RenderWidth / RenderHeight;
            return Math.Abs(DAR - renderAspect) <= 0.01; // allow a slight deviation as you don't get exactly the DAR with small frame sizes
        }
    }

    internal class Resolution {
        public int Width { get; set; }
        public int Height { get; set; }

        public Resolution(int width, int height) {
            Width = width;
            Height = height;
        }

        public Resolution CalculateResize(Resolution maxOutputSize, int framesizeMultipleOff = 1) {
            return CalculateResize(Width / Height, maxOutputSize, framesizeMultipleOff);
        }

        public Resolution CalculateResize(decimal destinationAspectRatio, Resolution maxOutput, int framesizeMultipleOff = 1) {
            // get the aspect ratio for the height / width calculation, defaulting to 16:9
            decimal displayAspect = destinationAspectRatio == 0 ? 16 / 9 : destinationAspectRatio;

            // calculate new width
            int width = maxOutput.Width;
            int height = (int)(width * (1 / displayAspect));
            if (height > maxOutput.Height) {
                height = maxOutput.Height;
                width = (int)(height * displayAspect);
            }

            // round
            int newWidth = ((int)Math.Round(width * 1.0 / framesizeMultipleOff)) * framesizeMultipleOff;
            int newHeight = ((int)Math.Round(height * 1.0 / framesizeMultipleOff)) * framesizeMultipleOff;

            return new Resolution(newWidth, newHeight);
        }

        public override string ToString() {
            return Width.ToString() + "x" + Height.ToString();
        }

        public static Resolution Calculate(decimal destinationAspectRatio, Resolution maxOutput, int framesizeMultipleOff = 1) {
            Resolution res = new Resolution(0, 0);
            return res.CalculateResize(destinationAspectRatio, maxOutput, framesizeMultipleOff);
        }
    }
}
