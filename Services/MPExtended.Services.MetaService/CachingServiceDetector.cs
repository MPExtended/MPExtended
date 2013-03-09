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
using MPExtended.Libraries.Service.Util;
using MPExtended.Services.MetaService.Interfaces;

namespace MPExtended.Services.MetaService
{
    internal class CachingServiceDetector : BaseServiceDetector
    {
        private IServiceDetector realDetector;

        private bool? doesHaveMAS { get; set; }
        private bool? doesHaveTAS { get; set; }
        private bool? doesHaveWSS { get; set; }
        private bool? doesHaveUI { get; set; }

        public override bool HasActiveMAS
        {
            get
            {
                if(!doesHaveMAS.HasValue)
                {
                    doesHaveMAS = realDetector.HasActiveMAS;
                }
                return doesHaveMAS.Value;
            }
        }

        public override bool HasActiveTAS
        {
            get
            {
                if(!doesHaveTAS.HasValue)
                {
                    doesHaveTAS = realDetector.HasActiveTAS;
                }
                return doesHaveTAS.Value;
            }
        }

        public override bool HasActiveWSS
        {
            get
            {
                if(!doesHaveWSS.HasValue)
                {
                    doesHaveWSS = realDetector.HasActiveWSS;
                }
                return doesHaveWSS.Value;
            }
        }

        public override bool HasUI
        {
            get
            {
                if(!doesHaveUI.HasValue)
                {
                    doesHaveUI = realDetector.HasUI;
                }
                return doesHaveUI.Value;
            }
        }

        public CachingServiceDetector(IServiceDetector realDetector, CompositionHinter hinter)
            : base (hinter)
        {
            this.realDetector = realDetector;
        }
    }
}
