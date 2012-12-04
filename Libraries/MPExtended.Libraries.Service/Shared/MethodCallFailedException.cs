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
using System.Text;

namespace MPExtended.Libraries.Service.Shared
{
    // This exception should be thrown by a method when the method call failed (and an exception should be send to the
    // client), but no failed method call should be logged. Usually the method itself logs more detailed error information
    // and the exception is redunant. It can also be used to return more detailed error information to the client.

    [Serializable]
    public class MethodCallFailedException : Exception
    {
        public MethodCallFailedException()
            : base ()
        {
        }

        public MethodCallFailedException(string message)
            : base (message)
        {
        }

        public MethodCallFailedException(string message, Exception innerException)
            : base (message, innerException)
        {
        }
    }
}
