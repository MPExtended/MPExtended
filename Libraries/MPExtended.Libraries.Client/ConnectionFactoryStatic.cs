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
using System.ServiceModel;
using System.Text;

namespace MPExtended.Libraries.Client
{
    public static class ConnectionFactory
    {
        public static void DisposeConnection(object connection)
        {
            try
            {
                if (connection != null && connection is ICommunicationObject)
                    (connection as ICommunicationObject).Close();
            }
            catch (Exception)
            {
                // Just ignore it, we're trying to get rid of the connection anyway
            }
        }
    }
}
