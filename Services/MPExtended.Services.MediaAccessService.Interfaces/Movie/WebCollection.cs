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
using System.Collections.Generic;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
  public class WebCollection : WebObject, ITitleSortable, IArtwork
  {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public IList<WebArtwork> Artwork { get; set; }

    public WebCollection()
    {
      Artwork = new List<WebArtwork>();
    }

    public WebCollection(string title) : this()
    {
      Title = title;
    }

    public override string ToString()
    {
      return Title;
    }

    public override bool Equals(object obj)
    {
      WebCollection r = obj is string ? new WebCollection((string)obj) : obj as WebCollection;
      return (object)r != null && this.Title == r.Title;
    }

    public override int GetHashCode()
    {
      return Title.GetHashCode();
    }

    public static bool operator ==(WebCollection a, WebCollection b)
    {
      return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Title == b.Title);
    }

    public static bool operator !=(WebCollection a, WebCollection b)
    {
      return !(a == b);
    }

    public static implicit operator WebCollection(string value)
    {
      return new WebCollection(value);
    }

    public static implicit operator string(WebCollection value)
    {
      return value.Title;
    }

    public WebMediaType Type
    {
      get
      {
        return WebMediaType.Collection;
      }
    }
  }
}
