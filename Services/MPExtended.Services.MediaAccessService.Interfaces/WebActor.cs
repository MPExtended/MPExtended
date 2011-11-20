using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebActor
    {
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj != null && obj is WebActor && (obj as WebActor).Name == this.Name;
        }

        public bool Equals(WebActor actor)
        {
            return actor.Name == this.Name;
        }
    }
}
