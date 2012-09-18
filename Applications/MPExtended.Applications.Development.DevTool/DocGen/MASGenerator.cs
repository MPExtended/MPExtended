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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MPExtended.Applications.Development.DevTool.DocGen
{
    internal class MASGenerator : Generator
    {
        private Assembly commonAssembly;

        public MASGenerator(Assembly assembly)
        {
            this.Assembly = assembly;
            this.JsonAPI = this.Assembly.GetType("MPExtended.Services.MediaAccessService.Interfaces.IMediaAccessService");

            commonAssembly = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(assembly.Location), "MPExtended.Services.Common.Interfaces.dll"));
        }

        protected override int GenerateSortOrder(string methodName)
        {
            if (methodName.Contains("Filter")) return 2;
            if (methodName.Contains("Movie")) return 3;
            if (methodName.Contains("Music")) return 4;
            if (methodName.Contains("Playlist")) return 5;
            if (methodName.Contains("Picture")) return 6;
            if (methodName.Contains("TV")) return 7;
            if (methodName.Contains("FileSystem")) return 8;
            return 1; // show unknown at top
        }

        protected override Dictionary<int, string> GetHeadings()
        {
            return new Dictionary<int, string>()
            {
                { 1, "General" },
                { 2, "Filters" },
                { 3, "Movie" },
                { 4, "Music" },
                { 5, "Playlist" },
                { 6, "Picture" },
                { 7, "TVShow" },
                { 8, "FileSystem" },
            };
        }

        protected override string MapName(MethodInfo method, string typename)
        {
            if (typename == "WebSortField")
            {
                Type rettype;
                IsListType(method.ReturnType, out rettype);
                var items = rettype.GetInterfaces().Where(x => x.Name.EndsWith("Sortable")).Select(x => x.Name.Substring(1, x.Name.Length - 9)).ToList();

                Type sby = commonAssembly.GetType("MPExtended.Services.Common.Interfaces.WebSortField");
                var valid = sby.GetEnumValues().Cast<int>().Select(x => new { Value = x, Name = sby.GetEnumName(x) }).Where(x => items.Contains(x.Name));

                var validTxt = String.Join(", ", valid.Select(x => x.Name + " = " + x.Value).ToArray());
                return "WebSortField; valid values: " + (validTxt.Length > 0 ? validTxt : "<em>none</em>");
            }

            return base.MapName(method, typename);
        }
    }
}
