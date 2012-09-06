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
using MPExtended.Libraries.Service.Shared.Filters;
using Xunit;

namespace MPExtended.Tests.Libraries.Service.Shared
{
    public class FilterTests
    {
        [Fact]
        public void Tokenizer()
        {
            Tokenizer tokenizer;
            List<string> tokens;

            tokenizer = new Tokenizer(@"Field=Value,AnotherField    $= 'AnotherValue'| NumericField> 5, DoubleQuoted ^= ""double quoted""");
            tokens = tokenizer.Tokenize();
            Assert.Equal("Field", tokens[0]);
            Assert.Equal("=", tokens[1]);
            Assert.Equal("Value", tokens[2]);
            Assert.Equal(",", tokens[3]);
            Assert.Equal("AnotherField", tokens[4]);
            Assert.Equal("$=", tokens[5]);
            Assert.Equal("AnotherValue", tokens[6]);
            Assert.Equal("|", tokens[7]);
            Assert.Equal("NumericField", tokens[8]);
            Assert.Equal(">", tokens[9]);
            Assert.Equal("5", tokens[10]);
            Assert.Equal(",", tokens[11]);
            Assert.Equal("DoubleQuoted", tokens[12]);
            Assert.Equal("^=", tokens[13]);
            Assert.Equal("double quoted", tokens[14]);
            Assert.Equal(15, tokens.Count);

            tokenizer = new Tokenizer(@"EscapedField='hoi\'test\\more'    ,ComplicatedField=""test\\\"""" | withoutQuotes=hello\,more");
            tokens = tokenizer.Tokenize();
            Assert.Equal("EscapedField", tokens[0]);
            Assert.Equal("=", tokens[1]);
            Assert.Equal(@"hoi'test\more", tokens[2]);
            Assert.Equal(",", tokens[3]);
            Assert.Equal("ComplicatedField", tokens[4]);
            Assert.Equal("=", tokens[5]);
            Assert.Equal(@"test\""", tokens[6]);
            Assert.Equal("|", tokens[7]);
            Assert.Equal("withoutQuotes", tokens[8]);
            Assert.Equal("=", tokens[9]);
            Assert.Equal("hello,more", tokens[10]);
            Assert.Equal(11, tokens.Count);

            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, NoValue=").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Missing'Operator'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Missing='Operator', InSecond'Field'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Unexpected=End,").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Unexpected=End,").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed='test', Invalid='Quoting").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed='test', Invalid='Quoting\'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=""test"", Invalid='Quoting""").Tokenize());
        }
    }
}
