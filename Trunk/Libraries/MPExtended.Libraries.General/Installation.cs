#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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

namespace MPExtended.Libraries.General
{
    public enum InstallationType
    {
        Client,
        Server,
        Singleseat
    }

    public static class Installation
    {
        public static bool HasLocalTAS
        {
            get
            {
                return true;
            }
        }

        public static bool HasLocalMAS
        {
            get
            {
                return true;
            }
        }

        public static string MASAddress
        {
            get
            {
                return "127.0.0.1";
            }
        }

        public static string TASAddress
        {
            get
            {
                return "127.0.0.1";
            }
        }

        public static InstallationType GetInstallationType()
        {
            return InstallationType.Singleseat;
        }
    }
}
