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

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
  public class WebTVShowGenre : WebGenre, ITitleSortable, IArtwork
  {

    public WebTVShowGenre() : base()
    {
    }

    public WebTVShowGenre(string name) : base(name)
    {
    }

    public override string ToString()
    {
        return Title;
    }

    public override bool Equals(object obj)
    {
      WebTVShowGenre r = obj is string ? new WebTVShowGenre((string)obj) : obj as WebTVShowGenre;
      return (object)r != null && this.Title == r.Title;
    }

    public override int GetHashCode()
    {
      return Title.GetHashCode();
    }

    public static bool operator ==(WebTVShowGenre a, WebTVShowGenre b)
    {
      return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Title == b.Title);
    }

    public static bool operator !=(WebTVShowGenre a, WebTVShowGenre b)
    {
      return !(a == b);
    }

    public static implicit operator WebTVShowGenre(string value)
    {
      return new WebTVShowGenre(value);
    }

    public static implicit operator string(WebTVShowGenre value)
    {
      return value.Title;
    }
  }
}
