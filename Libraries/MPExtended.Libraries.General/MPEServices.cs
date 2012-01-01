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
using System.ServiceModel;
using MPExtended.Services.MediaAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Libraries.General
{
    public static class MPEServices
    {
        private static MPEServicesHolder holder = new MPEServicesHolder(true);

        public static IMediaAccessService MAS { get { return holder.MAS; } }
        public static bool HasMASConnection { get { return holder.HasMASConnection; } }
        public static bool IsMASLocal { get { return holder.IsMASLocal; } }
        public static IWebStreamingService MASStreamControl { get { return holder.MASStreamControl; } }
        public static IStreamingService MASStream { get { return holder.MASStream; } }
        public static bool HasMASStreamConnection { get { return holder.HasMASStreamConnection; } }
        public static bool IsMASStreamLocal { get { return holder.IsMASStreamLocal; } }
        public static string HttpMASStreamRoot { get { return holder.HttpMASStreamRoot; } }

        public static ITVAccessService TAS { get { return holder.TAS; } }
        public static bool HasTASConnection { get { return holder.HasTASConnection; } }
        public static bool IsTASLocal { get { return holder.IsTASLocal; } }
        public static IWebStreamingService TASStreamControl { get { return holder.TASStreamControl; } }
        public static IStreamingService TASStream { get { return holder.TASStream; } }
        public static bool HasTASStreamConnection { get { return holder.HasTASStreamConnection; } }
        public static bool IsTASStreamLocal { get { return holder.IsTASStreamLocal; } }
        public static string HttpTASStreamRoot { get { return holder.HttpTASStreamRoot; } }

        public static void SetConnectionUrls(string mas, string tas)
        {
            holder = new MPEServicesHolder(mas, tas);
        }
    }
}