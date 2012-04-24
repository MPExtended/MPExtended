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
using System.Xml;
using MPExtended.Libraries.Service.Util;
using MPExtended.Libraries.Service;

namespace MediaPortal.Playlists
{
  public class PlayListWPLIO : IPlayListIO
  {
    public bool Load(PlayList playlist, string fileName, string label)
    {
      return Load(playlist, fileName);
    }

    public bool Load(PlayList playlist, string fileName)
    {
      playlist.Clear();

      try
      {
        string basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        if (doc.DocumentElement == null)
        {
          return false;
        }
        XmlNode nodeRoot = doc.DocumentElement.SelectSingleNode("/smil/body/seq");
        if (nodeRoot == null)
        {
          return false;
        }
        XmlNodeList nodeEntries = nodeRoot.SelectNodes("media");
        foreach (XmlNode node in nodeEntries)
        {
          XmlNode srcNode = node.Attributes.GetNamedItem("src");
          if (srcNode != null)
          {
            if (srcNode.InnerText != null)
            {
              if (srcNode.InnerText.Length > 0)
              {
                fileName = srcNode.InnerText;
                PathUtil.GetQualifiedFilename(basePath, ref fileName);
                PlayListItem newItem = new PlayListItem(fileName, fileName, 0);
                newItem.Type = PlayListItem.PlayListItemType.Audio;
                string description;
                description = Path.GetFileName(fileName);
                newItem.Description = description;
                playlist.Add(newItem);
              }
            }
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Info("exception loading playlist {0} err:{1} stack:{2}", fileName, ex.Message, ex.StackTrace);
      }
      return false;
    }

    public bool Save(PlayList playlist, string fileName)
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }
}