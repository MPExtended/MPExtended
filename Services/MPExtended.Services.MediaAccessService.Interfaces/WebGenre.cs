using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebGenre : WebObject, ITitleSortable
    {
        public string Title { get; set; }

        public override bool Equals(object obj)
        {
            return obj != null && obj is WebActor && (obj as WebGenre).Title == this.Title;
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode();
        }
    }
}
