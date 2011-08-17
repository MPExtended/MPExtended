using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.MediaAccessService.Interfaces.TVShow
{
    public class WebTVEpisodeDetailed : WebTVEpisodeBasic, IRatingSortable
    {
        ///<summary>The FirstAired property will return a DateTime object which indicates the time the episode was aired by broadcasting company  </summary>
        public DateTime FirstAired { get; set; }

        ///<summary>The LastViewPosition property will a float value which indicates the Position a user stopped playing this episode</summary>
        public float LastViewPosition { get; set; }
       

    }
}
