#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
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
using System.Runtime.InteropServices;
using System.Text;

// copy of MediaPortal SVN trunk/mediaportal/Core/Player/MediaInfo.cs r27491 as of 15 June 2011

namespace MPExtended.Services.StreamingService.MediaInfo
{
  internal enum StreamKind
  {
    General,
    Video,
    Audio,
    Text,
    Chapters,
    Image
  }

  internal enum InfoKind
  {
    Name,
    Text,
    Measure,
    Options,
    NameText,
    MeasureText,
    Info,
    HowTo
  }

  internal enum InfoOptions
  {
    ShowInInform,
    Support,
    ShowInSupported,
    TypeOfValue
  }


  internal class MediaInfo
  {
    //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)  
    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_New();

    [DllImport("MediaInfo.dll")]
    public static extern void MediaInfo_Delete(IntPtr Handle);

    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);

    [DllImport("MediaInfo.dll")]
    public static extern void MediaInfo_Close(IntPtr Handle);

    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);

    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter,
                                               IntPtr KindOfInfo);

    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber,
                                              [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo,
                                              IntPtr KindOfSearch);

    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option,
                                                 [MarshalAs(UnmanagedType.LPWStr)] string Value);

    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_State_Get(IntPtr Handle);

    [DllImport("MediaInfo.dll")]
    public static extern IntPtr MediaInfo_Count_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber);

    //MediaInfo class
    public MediaInfo()
    {
      Handle = MediaInfo_New();
    }

    ~MediaInfo()
    {
      MediaInfo_Delete(Handle);
    }

    public int Open(String FileName)
    {
      return (int)MediaInfo_Open(Handle, FileName);
    }

    public void Close()
    {
      MediaInfo_Close(Handle);
    }

    public String Inform()
    {
      return Marshal.PtrToStringUni(MediaInfo_Inform(Handle, (IntPtr)0));
    }

    public String Get(StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo,
                      InfoKind KindOfSearch)
    {
      return
        Marshal.PtrToStringUni(MediaInfo_Get(Handle, (IntPtr)StreamKind, (IntPtr)StreamNumber, Parameter,
                                             (IntPtr)KindOfInfo, (IntPtr)KindOfSearch));
    }

    public String Get(StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo)
    {
      return
        Marshal.PtrToStringUni(MediaInfo_GetI(Handle, (IntPtr)StreamKind, (IntPtr)StreamNumber, (IntPtr)Parameter,
                                              (IntPtr)KindOfInfo));
    }

    public String Option(String Option, String Value)
    {
      return Marshal.PtrToStringUni(MediaInfo_Option(Handle, Option, Value));
    }

    public int State_Get()
    {
      return (int)MediaInfo_State_Get(Handle);
    }

    public int Count_Get(StreamKind StreamKind, int StreamNumber)
    {
      return (int)MediaInfo_Count_Get(Handle, (IntPtr)StreamKind, (IntPtr)StreamNumber);
    }

    private IntPtr Handle;

    //Default values, if you know how to set default values in C#, say me
    public String Get(StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo)
    {
      return Get(StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name);
    }

    public String Get(StreamKind StreamKind, int StreamNumber, String Parameter)
    {
      return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name);
    }

    public String Get(StreamKind StreamKind, int StreamNumber, int Parameter)
    {
      return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text);
    }

    public String Option(String Option_)
    {
      return Option(Option_, "");
    }

    public int Count_Get(StreamKind StreamKind)
    {
      return Count_Get(StreamKind, -1);
    }
  }
}

//NameSpace