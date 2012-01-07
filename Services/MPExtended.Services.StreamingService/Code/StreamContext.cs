#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Text;
using MPExtended.Libraries.General;
using MPExtended.Libraries.General.ConfigurationContracts;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Services.StreamingService.Code
{
    internal class StreamContext
    {
        // time in milliseconds for which a client player sync position is valid
        private const int CLIENT_SYNC_VALID_DURATION = 120000;

        public MediaSource Source { get; set; }
        public WebMediaInfo MediaInfo { get; set; }
        public bool IsTv { get; set; }

        public Resolution OutputSize { get; set; }
        public TranscoderProfile Profile { get; set; }

        public int StartPosition { get; set; }
        public int SyncedPlayerPosition { get; set; }
        public DateTime LastPlayerPositionSync { get; set; }

        public Pipeline Pipeline { get; set; }

        public WebTranscodingInfo TranscodingInfo { get; set; }

        public int? AudioTrackId { get; set; }
        public int? SubtitleTrackId { get; set; }

        /// <summary>
        /// Get the approximate player position of the client. This value comes from the client, transcoder and clock.
        /// </summary>
        /// <returns>Client player position in milliseconds</returns>
        public int GetPlayerPosition()
        {
            if (LastPlayerPositionSync > DateTime.Now.Subtract(TimeSpan.FromMilliseconds(CLIENT_SYNC_VALID_DURATION)))
            {
                return SyncedPlayerPosition + (int)(DateTime.Now - LastPlayerPositionSync).TotalMilliseconds;
            }
            else if (TranscodingInfo != null)
            {
                return TranscodingInfo.TranscodingPosition;
            }
            else
            {
                return 0;
            }
        }
    }
}
