#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Libraries.Service.Extensions;
using MPExtended.Services.Common.Interfaces;
using Xunit;

namespace MPExtended.Tests.Libraries.Service.Extensions
{
    public class NaturalSorting
    {
        [Fact]
        public void Prefixes()
        {
            string[] list = new string[] {
                "The Something", "  The  Something Else", "An Working Example", "Without a prefix", "A B C", "Suffixed The Left Alone", 
                "Something ABC", "Worklll", "Final" 
            };

            var sorted = list.OrderByNatural(x => x, WebSortOrder.Asc).ToList();

            Assert.Equal(9, sorted.Count);
            Assert.Equal("A B C", sorted[0]);
            Assert.Equal("Final", sorted[1]);
            Assert.Equal("The Something", sorted[2]);
            Assert.Equal("Something ABC", sorted[3]);
            Assert.Equal("  The  Something Else", sorted[4]);
            Assert.Equal("Suffixed The Left Alone", sorted[5]);
            Assert.Equal("Without a prefix", sorted[6]);
            Assert.Equal("An Working Example", sorted[7]);
            Assert.Equal("Worklll", sorted[8]);
        }

        [Fact]
        public void Numericals()
        {
            string[] list = new string[] {
                "30 People", "8 Shoes", "  10 Developers", "The 4 Lists", "A list of 12 items", "A list of 56 movies", "A list of 56 series", 
                "A list of 101 projects", "The 74th item", "Something random", "74 windows", "74 Alphabets",
                "CASEINSEnsitiVE", "caseinsensitife"
            };

            var sorted = list.OrderByNatural(x => x, WebSortOrder.Asc).ToList();

            Assert.Equal(14, sorted.Count);
            Assert.Equal("The 4 Lists", sorted[0]);
            Assert.Equal("8 Shoes", sorted[1]);
            Assert.Equal("  10 Developers", sorted[2]);
            Assert.Equal("30 People", sorted[3]);
            Assert.Equal("74 Alphabets", sorted[4]);
            Assert.Equal("74 windows", sorted[5]);
            Assert.Equal("The 74th item", sorted[6]);
            Assert.Equal("caseinsensitife", sorted[7]);
            Assert.Equal("CASEINSEnsitiVE", sorted[8]);
            Assert.Equal("A list of 12 items", sorted[9]);
            Assert.Equal("A list of 56 movies", sorted[10]);
            Assert.Equal("A list of 56 series", sorted[11]);
            Assert.Equal("A list of 101 projects", sorted[12]);
            Assert.Equal("Something random", sorted[13]);
        }
    }
}
