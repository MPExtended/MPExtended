using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.Shared
{
    public interface WebMediaItem
    {
         string Id { get; set; }
         string Path { get; set; }
         WebMediaType Type { get; set; }
    }
}
