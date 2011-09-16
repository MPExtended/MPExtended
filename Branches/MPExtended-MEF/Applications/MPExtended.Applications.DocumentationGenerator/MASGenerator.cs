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
using System.Reflection;

namespace MPExtended.Applications.DocumentationGenerator
{
    internal class MASGenerator : Generator
    {
        public MASGenerator(Assembly assembly)
        {
            this.Assembly = assembly;
            this.JsonAPI = this.Assembly.GetType("MPExtended.Services.MediaAccessService.Interfaces.IMediaAccessService");
            this.Enums = new List<Type>() {
                this.Assembly.GetType("MPExtended.Services.MediaAccessService.Interfaces.OrderBy"),
                this.Assembly.GetType("MPExtended.Services.MediaAccessService.Interfaces.SortBy"),
                this.Assembly.GetType("MPExtended.Services.MediaAccessService.Interfaces.WebMediaType")
            };
        }

        protected override int GenerateSortOrder(string methodName)
        {
            if (methodName.Contains("Movie")) return 2;
            if (methodName.Contains("Music")) return 3;
            if (methodName.Contains("Picture")) return 4;
            if (methodName.Contains("TV")) return 5;
            if (methodName.Contains("FileSystem")) return 6;
            return 1; // show unknown at first
        }

        protected override Dictionary<int, string> GetHeadings()
        {
            return new Dictionary<int, string>()
            {
                { 1, "General" },
                { 2, "Movie" },
                { 3, "Music" },
                { 4, "Picture" },
                { 5, "TV" },
                { 6, "FileSystem" },
            };
        }

        protected override string MapName(MethodInfo method, string typename)
        {
            if (typename == "SortBy")
            {
                Type rettype;
                IsListType(method.ReturnType, out rettype);
                var items = rettype.GetInterfaces().Where(x => x.Name.EndsWith("Sortable")).Select(x => x.Name.Substring(1, x.Name.Length - 9)).ToList();
                if (items.Contains("Category")) items[items.IndexOf("Category")] = "UserDefinedCategories";

                Type sby = Assembly.GetType("MPExtended.Services.MediaAccessService.Interfaces.SortBy");
                var valid = sby.GetEnumValues().Cast<int>().Select(x => new { Value = x, Name = sby.GetEnumName(x) }).Where(x => items.Contains(x.Name));

                return "<a href=\"#SortBy\">SortBy</a>; valid values: " + String.Join(", ", valid.Select(x => x.Name + " = " + x.Value).ToArray());
            }

            return base.MapName(method, typename);
        }
    }
}
