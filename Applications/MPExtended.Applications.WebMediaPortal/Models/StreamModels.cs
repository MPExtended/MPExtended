#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class PlayerViewModel
    {
        public IEnumerable<string> Transcoders { get; set; }
        public string Transcoder { get; set; }
        public VideoPlayer Player { get; set; }
        public string PlayerViewName { get; set; }
        public WebResolution Size { get; set; }
        public string URL { get; set; }

        public IEnumerable<SelectListItem> TranscoderSelectList
        {
            get
            {
                return Transcoders.Select(tc => new SelectListItem()
                    {
                        Text = tc,
                        Value = tc,
                        Selected = tc == Transcoder
                    });
            }
        }
    }

    public enum VideoPlayer
    {
        Flash,
        VLC
    }
}