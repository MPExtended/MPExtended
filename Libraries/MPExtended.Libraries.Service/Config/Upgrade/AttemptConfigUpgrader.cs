#region Copyright (C) 2012-2013 MPExtended
// Copyright (C) 2012-2013 MPExtended Developers, http://www.mpextended.com/
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

namespace MPExtended.Libraries.Service.Config.Upgrade
{
    internal abstract class AttemptConfigUpgrader<TModel> : ConfigUpgrader<TModel>
    {
        private TModel _model;

        public override bool CanUpgrade()
        {
            try
            {
                _model = DoUpgrade();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override TModel PerformUpgrade()
        {
            return _model;
        }

        protected abstract TModel DoUpgrade();
    }
}
