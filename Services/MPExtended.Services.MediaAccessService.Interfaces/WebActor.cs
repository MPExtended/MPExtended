using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebActor : WebObject, INameSortable
    {
        public string Name { get; set; }

        public WebActor()
        {
        }

        public WebActor(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            WebActor r = obj is string ? new WebActor((string)obj) : obj as WebActor;
            return (object)r != null && this.Name == r.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(WebActor a, WebActor b)
        {
            return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Name == b.Name);
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
            return value.Name;
        }
    }
}
