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
using System.Web;
using System.Web.Mvc;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    internal class JQueryDocumentReadyScript
    {
        private List<string> scriptBlocks = new List<string>();

        public void AddBlock(string block)
        {
            scriptBlocks.Add(block);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("jQuery(document).ready(function () {");
            foreach (var block in scriptBlocks)
            {
                builder.AppendLine(block);
            }
            builder.AppendLine("});");
            return builder.ToString();
        }
    }
}