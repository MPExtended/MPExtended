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
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class Resolution : WebResolution
    {
        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return Width.ToString() + "x" + Height.ToString();
        }

        public Resolution CalculateResize(Resolution maxOutputSize)
        {
            return Calculate(Width, Height, maxOutputSize.Width, maxOutputSize.Height, 1);
        }

        public Resolution CalculateResize(Resolution maxOutputSize, int framesizeMultipleOff)
        {
            return Calculate(Width, Height, maxOutputSize.Width, maxOutputSize.Height, framesizeMultipleOff);
        }

        public static Resolution Calculate(decimal destinationAspectRatio, Resolution maxOutput)
        {
            return Calculate(destinationAspectRatio, maxOutput.Width, maxOutput.Height);
        }

        public static Resolution Calculate(decimal destinationAspectRatio, int? maxWidth, int? maxHeight)
        {
            return Calculate(destinationAspectRatio, maxWidth, maxHeight, 1);
        }

        public static Resolution Calculate(int sourceWidth, int sourceHeight, int? maxWidth, int? maxHeight, int framesizeMultipleOff)
        {
            Resolution res = Calculate((decimal)sourceWidth / (decimal)sourceHeight, maxWidth, maxHeight, framesizeMultipleOff);
            if (res.Width == 0 && res.Height == 0)
            {
                return new Resolution(sourceWidth, sourceHeight);
            }
            return res;
        }

        public static Resolution Calculate(decimal destinationAspectRatio, int? maxWidth, int? maxHeight, int framesizeMultipleOff)
        {
            // get the aspect ratio for the height / width calculation, defaulting to 16:9
            decimal displayAspect = destinationAspectRatio == 0 ? 16 / 9 : destinationAspectRatio;

            // skip no resize situation
            if (maxWidth == null && maxHeight == null)
            {
                return new Resolution(0, 0);
            }

            // default to max size
            int width = maxWidth.HasValue ? maxWidth.Value : 0;
            int height = maxHeight.HasValue ? maxHeight.Value : 0;

            // correct aspect ratio if needed
            if (maxWidth == null)
            {
                width = (int)(height * displayAspect);
            }
            else
            {
                height = (int)(width * (1 / displayAspect));
                if (maxHeight != null && height > maxHeight)
                {
                    height = maxHeight.Value;
                    width = (int)(height * displayAspect);
                }
            }

            // round on frame multiple
            int newWidth = ((int)Math.Round(width * 1.0 / framesizeMultipleOff)) * framesizeMultipleOff;
            int newHeight = ((int)Math.Round(height * 1.0 / framesizeMultipleOff)) * framesizeMultipleOff;

            return new Resolution(newWidth, newHeight);
        }
    }
}
