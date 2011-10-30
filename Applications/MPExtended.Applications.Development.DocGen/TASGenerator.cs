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
using System.Text;
using System.Reflection;

namespace MPExtended.Applications.Development.DocGen
{
    internal class TASGenerator : Generator
    {
        public TASGenerator(Assembly assembly)
        {
            this.Assembly = assembly;
            this.JsonAPI = this.Assembly.GetType("MPExtended.Services.TVAccessService.Interfaces.ITVAccessService");
            this.Enums = new List<Type>() {
                //this.Assembly.GetType("MPExtended.Services.TVAccessService.Interfaces.WebChannelState.States")
            };
        }

        protected override int GenerateSortOrder(string methodName)
        {
            if (methodName.Contains("GetChannelState")) return 2;
            if (methodName.Contains("Switch")) return 5;
            if (methodName.Contains("Radio")) return 4;
            if (methodName.Contains("GetChannel")) return 3;
            if (methodName.Contains("GetGroup")) return 3;
            if (methodName.Contains("TimeShifting")) return 5;
            if (methodName.Contains("Heartbeat")) return 5;
            if (methodName.Contains("Schedule")) return 6;
            if (methodName.Contains("Recording")) return 7;
            if (methodName.Contains("Program")) return 8;

            return 1;
        }

        protected override Dictionary<int, string> GetHeadings()
        {
            return new Dictionary<int, string>()
            {
                { 1, "Misc" },
                { 2, "Channels" },
                { 3, "TV" },
                { 4, "Radio" },
                { 5, "Timeshiftings" },
                { 6, "Schedule" },
                { 7, "Recording" },
                { 8, "EPG" }
            };
        }
    }
}
