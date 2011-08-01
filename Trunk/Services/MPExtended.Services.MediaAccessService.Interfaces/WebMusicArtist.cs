using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
   
    public class WebMusicArtist
    {
        public string Title { get; set; }
        //public string[] Label { get; set; }
        //public string Year { get; set; }
        //public string[] Discographie { get; set; }
         //List<WebMusicAlbum> albumList = new List<WebMusicAlbum>();

        public WebMusicArtist()
        { }
        public WebMusicArtist(string title)
        {
            this.Title = title;

        }
        //public List<WebMusicAlbum> AlbumList
        //{
        //    get
        //    {
        //        return albumList;
        //    }
        //    set
        //    {
        //        albumList = value;
        //    }

        //}
    }
}