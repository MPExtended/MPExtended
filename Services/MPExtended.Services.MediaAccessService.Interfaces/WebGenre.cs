using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    public class WebGenre : WebObject, ITitleSortable
    {
        public string Title { get; set; }
    }
}
