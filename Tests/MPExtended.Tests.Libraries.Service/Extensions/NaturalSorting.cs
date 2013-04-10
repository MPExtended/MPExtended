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
                "The Something", "  The  Something Else", "A Working Example", "Without a prefix", "A B C", "Suffixed The Left Alone", 
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
            Assert.Equal("A Working Example", sorted[7]);
            Assert.Equal("Worklll", sorted[8]);
        }

        [Fact]
        public void Numericals()
        {
            string[] list = new string[] {
                "30 People", "8 Shoes", "  10 Developers", "The 4 Lists", "A list of 12 items", "A list of 56 movies", "A list of 56 series", 
                "A list of LVI episodes", "A list of 101 projects", "The 74th item", "Something random", "74 windows", "74 Alphabets",
                "CASEINSEnsitiVE", "caseinsensitife", "III Roman", "Roman XIV", "Roman CVLI", "Roman 147", "Roman CLC",
                "Total I", "Total II", "Total III", "Total IV", "Total V", "Total VI", "Total VII", "Total VIII", "Total IX", "Total X", "Total XI"
            };

            var sorted = list.OrderByNatural(x => x, WebSortOrder.Asc).ToList();

            Assert.Equal(31, sorted.Count);
            Assert.Equal("III Roman", sorted[0]);
            Assert.Equal("The 4 Lists", sorted[1]);
            Assert.Equal("8 Shoes", sorted[2]);
            Assert.Equal("  10 Developers", sorted[3]);
            Assert.Equal("30 People", sorted[4]);
            Assert.Equal("74 Alphabets", sorted[5]);
            Assert.Equal("74 windows", sorted[6]);
            Assert.Equal("The 74th item", sorted[7]);
            Assert.Equal("caseinsensitife", sorted[8]);
            Assert.Equal("CASEINSEnsitiVE", sorted[9]);
            Assert.Equal("A list of 12 items", sorted[10]);
            Assert.Equal("A list of LVI episodes", sorted[11]);
            Assert.Equal("A list of 56 movies", sorted[12]);
            Assert.Equal("A list of 56 series", sorted[13]);
            Assert.Equal("A list of 101 projects", sorted[14]);
            Assert.Equal("Roman XIV", sorted[15]);  // 14
            Assert.Equal("Roman CVLI", sorted[16]); // 146
            Assert.Equal("Roman 147", sorted[17]);
            Assert.Equal("Roman CLC", sorted[18]);  // 150
            Assert.Equal("Something random", sorted[19]);
            Assert.Equal("Total I", sorted[20]);
            Assert.Equal("Total II", sorted[21]);
            Assert.Equal("Total III", sorted[22]);
            Assert.Equal("Total IV", sorted[23]);
            Assert.Equal("Total V", sorted[24]);
            Assert.Equal("Total VI", sorted[25]);
            Assert.Equal("Total VII", sorted[26]);
            Assert.Equal("Total VIII", sorted[27]);
            Assert.Equal("Total IX", sorted[28]);
            Assert.Equal("Total X", sorted[29]);
            Assert.Equal("Total XI", sorted[30]);
        }
    }
}
