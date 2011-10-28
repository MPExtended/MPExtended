using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.FileSystem
{
    public class WebFolderBasic : WebObject, IDateAddedSortable
    {
        public WebFolderBasic()
        {
            DateAdded = new DateTime(1970, 1, 1);
        }

        public string Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
