using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebItemCount
    {
        public int Count { get; set; }

        public override bool Equals(object obj)
        {
            return (obj is Int32 && (Int32)obj == Count) || (obj is WebItemCount && (obj as WebItemCount).Count == this.Count);
        }

        public override int GetHashCode()
        {
            return Count;
        }
    }
}
