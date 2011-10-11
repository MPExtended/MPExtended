#region Copyright (C) 2010-2011 TV4Home, 2011 MPExtended
// Copyright (C) 2010-2011 TV4Home, http://tv4home.codeplex.com/
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
using Gentle.Provider.MySQL;
using Gentle.Provider.SQLServer;

namespace MPExtended.Services.TVAccessService
{
    /// <summary>
    /// This is required for VS to pick up the reference to Gentle. Ignore it.
    /// </summary>
    class GentleProviders
    {
        private GentleProviders()
        {
            MySQLProvider prov1 = new MySQLProvider("");
            SQLServerProvider prov2 = new SQLServerProvider("");
        }
    }
}
