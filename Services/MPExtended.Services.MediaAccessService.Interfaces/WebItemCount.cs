using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebItemCount
    {
        public int Count { get; set; }

        public WebItemCount()
        {
        }

        public WebItemCount(int value)
        {
            this.Count = value;
        }

        public override bool Equals(object obj)
        {
            WebItemCount c = obj is int ? new WebItemCount((int)obj) : obj as WebItemCount;
            return (object)c != null && this.Count == c.Count;
        }

        public override int GetHashCode()
        {
            return Count;
        }

        public static bool operator ==(WebItemCount a, WebItemCount b)
        {
            return Object.ReferenceEquals(a, b) || (((object)a) != null && ((object)b) != null && a.Count == b.Count);
        }

        public static bool operator !=(WebItemCount a, WebItemCount b)
        {
            return !(a == b);
        }

        public static implicit operator int(WebItemCount count)
        {
            return count.Count;
        }

        public static implicit operator WebItemCount(int count)
        {
            return new WebItemCount(count);
        }
    }
}
