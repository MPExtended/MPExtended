#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Config;
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

        public long StartPosition { get; set; }
        public long SyncedPlayerPosition { get; set; }
        public DateTime LastPlayerPositionSync { get; set; }

        public Pipeline Pipeline { get; set; }

        public WebTranscodingInfo TranscodingInfo { get; set; }

        public int? AudioTrackId { get; set; }
        public int? SubtitleTrackId { get; set; }

        /// <summary>
        /// Get the approximate player position of the client. This value comes from the client, transcoder and clock.
        /// </summary>
        /// <returns>Client player position in milliseconds</returns>
        public long GetPlayerPosition()
        {
            if (LastPlayerPositionSync > DateTime.Now.Subtract(TimeSpan.FromMilliseconds(CLIENT_SYNC_VALID_DURATION)))
            {
                return SyncedPlayerPosition + (long)(DateTime.Now - LastPlayerPositionSync).TotalMilliseconds;
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
