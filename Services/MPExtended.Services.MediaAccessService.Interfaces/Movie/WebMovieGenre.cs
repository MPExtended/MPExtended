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

namespace MPExtended.Services.MediaAccessService.Interfaces.Movie
{
  public class WebMovieGenre : WebGenre, ITitleSortable, IArtwork
  {

    public WebMovieGenre() : base()
    {
    }

    public WebMovieGenre(string name) : base(name)
    {
    }

    public override string ToString()
    {
        return Title;
    }

    public override bool Equals(object obj)
    {
      WebMovieGenre r = obj is string ? new WebMovieGenre((string)obj) : obj as WebMovieGenre;
      return (object)r != null && this.Title == r.Title;
    }

    public override int GetHashCode()
    {
      return Title.GetHashCode();
    }

    public static bool operator ==(WebMovieGenre a, WebMovieGenre b)
    {
      return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Title == b.Title);
    }

    public static bool operator !=(WebMovieGenre a, WebMovieGenre b)
    {
      return !(a == b);
    }

    public static implicit operator WebMovieGenre(string value)
    {
      return new WebMovieGenre(value);
    }

    public static implicit operator string(WebMovieGenre value)
    {
      return value.Title;
    }
  }
}
