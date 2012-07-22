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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Text;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Util;

namespace MediaPortal.Playlists
{
    public class PlayListM3uIO : IPlayListIO
  {
    private const string M3U_START_MARKER = "#EXTM3U";
    private const string M3U_INFO_MARKER = "#EXTINF";
    private PlayList playlist;
    private StreamReader file;
    private string basePath;

    public PlayListM3uIO() {}

    public bool Load(PlayList playlist, string fileName, string label)
    {
      return Load(playlist, fileName);
    }

    public bool Load(PlayList incomingPlaylist, string playlistFileName)
    {
      if (playlistFileName == null)
      {
        return false;
      }
      playlist = incomingPlaylist;
      playlist.Clear();

      try
      {
        playlist.Name = Path.GetFileName(playlistFileName);
        basePath = Path.GetDirectoryName(Path.GetFullPath(playlistFileName));

        using (file = new StreamReader(playlistFileName, Encoding.Default, true))
        {
          if (file == null)
          {
            return false;
          }

          string line = file.ReadLine();
          if (line == null || line.Length == 0)
          {
            return false;
          }

          string trimmedLine = line.Trim();

          if (trimmedLine != M3U_START_MARKER)
          {
            string fileName = trimmedLine;
            if (!AddItem("", 0, fileName))
            {
              return false;
            }
          }

          line = file.ReadLine();
          while (line != null)
          {
            trimmedLine = line.Trim();

            if (trimmedLine != "")
            {
              if (trimmedLine.StartsWith(M3U_INFO_MARKER))
              {
                string songName = null;
                int lDuration = 0;

                if (ExtractM3uInfo(trimmedLine, ref songName, ref lDuration))
                {
                  line = file.ReadLine();
                  if (!AddItem(songName, lDuration, line))
                  {
                    break;
                  }
                }
              }
              else
              {
                if (!AddItem("", 0, trimmedLine))
                {
                  break;
                }
              }
            }
            line = file.ReadLine();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("exception loading playlist {0} err:{1} stack:{2}", playlistFileName, ex.Message, ex.StackTrace);
        return false;
      }
      return true;
    }

    private static bool ExtractM3uInfo(string trimmedLine, ref string songName, ref int lDuration)
    {
      //bool successfull;
      int iColon = (int)trimmedLine.IndexOf(":");
      int iComma = (int)trimmedLine.IndexOf(",");
      if (iColon >= 0 && iComma >= 0 && iComma > iColon)
      {
        iColon++;
        string duration = trimmedLine.Substring(iColon, iComma - iColon);
        iComma++;
        songName = trimmedLine.Substring(iComma);
        lDuration = Int32.Parse(duration);
        return true;
      }
      return false;
    }


    private bool AddItem(string songName, int duration, string fileName)
    {
      if (fileName == null || fileName.Length == 0)
      {
        return false;
      }

      PlayListItem newItem = new PlayListItem(songName, fileName, duration);
      if (fileName.ToLower().StartsWith("http:") || fileName.ToLower().StartsWith("https:") ||
          fileName.ToLower().StartsWith("mms:") || fileName.ToLower().StartsWith("rtp:"))
      {
        newItem.Type = PlayListItem.PlayListItemType.AudioStream;
      }
      else
      {
        fileName = PathUtil.GetAbsolutePath(basePath, fileName);
        newItem.FileName = fileName;
        newItem.Type = PlayListItem.PlayListItemType.Audio;
      }
      if (songName.Length == 0)
      {
        newItem.Description = Path.GetFileName(fileName);
      }
      playlist.Add(newItem);
      return true;
    }

    public bool Save(PlayList playlist, string fileName)
    {
      try
      {
        using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
        {
          writer.WriteLine(M3U_START_MARKER);

          foreach (PlayListItem item in playlist)
          {
            writer.WriteLine("{0}:{1},{2}", M3U_INFO_MARKER, item.Duration, item.Description);
            writer.WriteLine("{0}", item.FileName);
          }
        }
        return true;
      }
      catch (Exception e)
      {
        Log.Info("failed to save a playlist {0}. err: {1} stack: {2}", fileName, e.Message, e.StackTrace);
        return false;
      }
    }
  }
}