#region Copyright (C) 2012-2013 MPExtended, 2020 Team MediaPortal
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebActor : WebObject, ITitleSortable, IArtwork
    {
        public string Title { get; set; }
        public string IMDBId { get; set; }
        public IList<WebArtwork> Artwork { get; set; }
        
        public WebActor()
        {
          Artwork = new List<WebArtwork>();
        }

        public WebActor(string name) : this()
        {
            Title = name;
        }

        public override string ToString()
        {
            return Title;
        }

        public override bool Equals(object obj)
        {
            WebActor r = obj is string ? new WebActor((string)obj) : obj as WebActor;
            return (object)r != null && this.Title == r.Title;
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
        }

        public static bool operator ==(WebActor a, WebActor b)
        {
            return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Title == b.Title);
        }

        public static bool operator !=(WebActor a, WebActor b)
        {
            return !(a == b);
        }

        public static implicit operator WebActor(string value)
        {
            return new WebActor(value);
        }

        public static implicit operator string(WebActor value)
        {
            return value.Title;
        }
    }
}
