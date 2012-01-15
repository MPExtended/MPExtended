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
using MPExtended.Libraries.Service;
using System.Windows;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal static class ErrorHandling
    {
        public static void ShowError(Exception ex)
        {
            Log.Warn("Unexpected error happened", ex);
            string message = String.Format("An unexpected error occured. Please file a bugreport with the service's log files attached.\n\n{0}", ex.Message);
            MessageBox.Show(message, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
