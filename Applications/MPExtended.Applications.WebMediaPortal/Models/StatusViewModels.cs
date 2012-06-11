#region Copyright (C) 2012 MPExtended
// Copyright (C) 2012 MPExtended Developers, http://mpextended.github.com/
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
using System.Web;
using System.Management;
using MPExtended.Applications.WebMediaPortal.Strings;
using MPExtended.Libraries.Client;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public class StatusViewModel
    {
        public IEnumerable<TVCardViewModel> Cards { get; set; }
        public IEnumerable<WebDiskSpaceInformation> DiskInformation { get; set; }
        public long TotalMemoryMegaBytes { get; set; }

        public StatusViewModel(IEnumerable<WebCard> cards, IEnumerable<WebVirtualCard> activeCards, IEnumerable<WebDiskSpaceInformation> recordingDiskInfo)
        {
            DiskInformation = recordingDiskInfo;
            TotalMemoryMegaBytes = GetTotalMemoryBytes() / 1024 / 1024;

            // cards
            Cards = new List<TVCardViewModel>();

            foreach (var card in cards.OrderBy(c => c.IdCard))
            {
                var match = activeCards.Where(vc => vc.Id == card.IdCard);
                if (match.Any())
                {
                    ((List<TVCardViewModel>)Cards).AddRange(match.Select(vc => new TVCardViewModel(card, vc)));
                }
                else
                {
                    ((List<TVCardViewModel>)Cards).Add(new TVCardViewModel(card));
                }
            }
        }

        internal static long GetTotalMemoryBytes()
        {
            ObjectQuery objectQuery = new ObjectQuery("SELECT TotalPhysicalMemory from Win32_ComputerSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(objectQuery);
            var enumerator = searcher.Get().GetEnumerator();
            if (!enumerator.MoveNext()) return 0;
            return Convert.ToInt64(enumerator.Current["TotalPhysicalMemory"]);
        }
    }

    public class TVCardViewModel
    {
        public int CardId { get; private set; }
        public string Name { get; private set; }
        public string State { get; private set; }

        public bool IsActive { get; private set; }
        public WebVirtualCard VirtualCard { get; private set; }

        public bool GrabbingEPG { get; private set; }
        public bool Scrambled { get; private set; }
        public string Username { get; private set; }
        public int ChannelId { get; private set; }

        public string ChannelName
        {
            get
            {
                return ChannelId > 0 ? MPEServices.TAS.GetChannelDetailedById(ChannelId).DisplayName : String.Empty;
            }
        }

        public TVCardViewModel(WebCard card)
        {
            CardId = card.IdCard;
            Name = card.Name;

            IsActive = false;
            State = FormStrings.StateIdle;
            GrabbingEPG = false;
            Scrambled = false;
            ChannelId = 0;
        }

        public TVCardViewModel(WebCard card, WebVirtualCard activeCard)
            : this(card)
        {
            IsActive = true;
            VirtualCard = activeCard;

            GrabbingEPG = activeCard.IsGrabbingEpg;
            Scrambled = activeCard.IsScrambled;
            Username = activeCard.User.Name;
            ChannelId = activeCard.IdChannel;

            if (activeCard.IsTimeShifting) State = FormStrings.StateTimeshifting;
            if (activeCard.IsRecording) State = FormStrings.StateRecording;
            if (activeCard.IsScanning) State = FormStrings.StateScanning;
        }

        public IEnumerable<string> GetRTSPClients()
        {
            if (!IsActive) 
                return new List<string>();

            Uri uri = new Uri(VirtualCard.RTSPUrl);
            return MPEServices.TAS.GetStreamingClients()
                .Where(client => client.StreamName == uri.LocalPath.Substring(1))
                .Select(client => client.IpAdress);
        }

        public IEnumerable<string> GetMPExtendedClients()
        {
            return MPEServices.TASStreamControl.GetStreamingSessions()
                .Where(x => x.SourceType == WebStreamMediaType.TV && VirtualCard.User.Name == "mpextended-" + x.Identifier)
                .Select(x => x.ClientIPAddress);
        }

        public IEnumerable<string> GetAllClients()
        {
            return GetRTSPClients().Concat(GetMPExtendedClients());
        }
    }
}