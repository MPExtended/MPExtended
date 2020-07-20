#region Copyright (C) 2020 Team MediaPortal
// Copyright (C) 2020 Team MediaPortal, http://www.team-mediaportal.com/
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
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Services.MediaAccessService.Interfaces.Picture
{
  public class WebMobileVideoBasic : WebMediaItem, IDateAddedSortable, IPictureDateTakenSortable, ICategorySortable
  {
    public WebMobileVideoBasic()
    {
      DateTaken = new DateTime(1970, 1, 1);
      Categories = new List<WebCategory>();
    }

    public IList<WebCategory> Categories { get; set; }
    public DateTime DateTaken { get; set; }
    public string Description { get; set; }

    public override WebMediaType Type
    {
      get
      {
        return WebMediaType.MobileVideo;
      }
    }

    public override string ToString()
    {
      return Title;
    }
  }
}
