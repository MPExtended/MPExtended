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
            return obj as WebItemCount != null && (obj as WebItemCount).Count == this.Count;
        }

        public override int GetHashCode()
        {
            return Count;
        }

        public static bool operator ==(WebItemCount a, WebItemCount b)
        {
            return a.Count == b.Count;
        }

        public static bool operator !=(WebItemCount a, WebItemCount b)
        {
            return a.Count != b.Count;
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
