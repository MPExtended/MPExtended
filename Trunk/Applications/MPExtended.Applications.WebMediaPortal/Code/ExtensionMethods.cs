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
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Xml;

namespace MPExtended.Applications.WebMediaPortal.Code
{
    public static class ExtensionMethods
    {
        public static XmlNode GetFirstNode(this XmlNodeList list, string nodeName)
        {
            foreach (XmlNode node in list)
            {
                if (node.Name == nodeName) return node;
            }
            return null;
        }
    }
}