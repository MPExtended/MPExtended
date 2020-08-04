#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Linq;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
  public class WebTVShowActor : WebActor, ITitleSortable, IArtwork
  {
    public string TVDBId
    {
      get
      {
        return ExternalId.Where(x => x.Site == "TVDB").FirstOrDefault()?.Id ?? string.Empty;
      }
    }

    public WebTVShowActor() : base()
    {
    }

    public WebTVShowActor(string name) : base(name)
    {
    }

    public override string ToString()
    {
        return Title;
    }

    public override bool Equals(object obj)
    {
      WebTVShowActor r = obj is string ? new WebTVShowActor((string)obj) : obj as WebTVShowActor;
      return (object)r != null && this.Title == r.Title;
    }

    public override int GetHashCode()
    {
      return Title.GetHashCode();
    }

    public static bool operator ==(WebTVShowActor a, WebTVShowActor b)
    {
      return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Title == b.Title);
    }

    public static bool operator !=(WebTVShowActor a, WebTVShowActor b)
    {
      return !(a == b);
    }

    public static implicit operator WebTVShowActor(string value)
    {
      return new WebTVShowActor(value);
    }

    public static implicit operator string(WebTVShowActor value)
    {
      return value.Title;
    }
  }
}
