#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.EXIF;

namespace MPExtended.Services.StreamingService.Code
{
  internal class EXIFInfoHelper
  {
    public static WebEXIFInfo LoadEXIFInfoOrSurrogate(MediaSource source)
    {
      WebEXIFInfo info;
      try
      {
        info = EXIFInfoWrapper.GetExifInfo(source);
        if (info != null)
        {
          return info;
        }
      }
      catch (Exception ex)
      {
        Log.Warn(String.Format("Failed to load EXIF for {0}", source.GetDebugName()), ex);
      }

      return new WebEXIFInfo();
    }
  }
}
