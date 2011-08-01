//Copyright 2010 http://tv4home.codeplex.com
//This file is part of MPExtended.Services.TVAccessService.Interfaces.
//MPExtended.Services.TVAccessService.Interfaces is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 2 of the License, or (at your option) any later version.
//MPExtended.Services.TVAccessService.Interfaces is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU Lesser General Public License along with MPExtended.Services.TVAccessService.Interfaces. If not, see http://www.gnu.org/licenses/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.TVAccessService.Interfaces
{
    public class WebCard
    {
        #region Properties
        public bool CAM { get; set; }
        public int CamType { get; set; }
        public int DecryptLimit { get; set; }
        public string DevicePath { get; set; }
        public bool Enabled { get; set; }
        public bool GrabEPG { get; set; }
        public int IdCard { get; set; }
        public int IdServer { get; set; }
        public bool IsChanged { get; set; }
        public DateTime LastEpgGrab { get; set; }
        public string Name { get; set; }
        public int netProvider { get; set; }
        public bool PreloadCard { get; set; }
        public int Priority { get; set; }
        public string RecordingFolder { get; set; }
        public int RecordingFormat { get; set; }
        public bool supportSubChannels { get; set; }
        public string TimeShiftFolder { get; set; }
        #endregion
    }
}
