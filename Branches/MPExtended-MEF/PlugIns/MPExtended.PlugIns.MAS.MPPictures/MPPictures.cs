using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.MediaAccessService.Interfaces.Picture;

namespace MPExtended.PlugIns.MAS.MPPictures
{
    [Export(typeof(IPictureLibrary))]
    [ExportMetadata("Database", "MPPictures")]
    public class MPPictures :IPictureLibrary
    {
        public IList<WebPictureBasic> GetAllPicturesBasic()
        {
            throw new NotImplementedException();
        }

        public IList<WebPictureDetailed> GetAllPicturesDetailed()
        {
            throw new NotImplementedException();
        }

        public WebPictureDetailed GetPictureDetailed(string pictureId)
        {
            throw new NotImplementedException();
        }

        public IList<WebPictureCategoryBasic> GetAllPictureCategoriesBasic()
        {
            throw new NotImplementedException();
        }

        public System.IO.DirectoryInfo GetSourceRootDirectory()
        {
            throw new NotImplementedException();
        }
    }
}
