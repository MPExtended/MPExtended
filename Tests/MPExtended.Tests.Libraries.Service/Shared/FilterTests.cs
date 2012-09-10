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

            tokenizer = new Tokenizer(@"Field=Name With Whitespace,   AnotherField ^=  Prefixed and Suffixed space   | QuotedField='   Includes space  '");
            tokens = tokenizer.Tokenize();
            Assert.Equal("Field", tokens[0]);
            Assert.Equal("=", tokens[1]);
            Assert.Equal("Name With Whitespace", tokens[2]);
            Assert.Equal(",", tokens[3]);
            Assert.Equal("AnotherField", tokens[4]);
            Assert.Equal("^=", tokens[5]);
            Assert.Equal("Prefixed and Suffixed space", tokens[6]);
            Assert.Equal("|", tokens[7]);
            Assert.Equal("QuotedField", tokens[8]);
            Assert.Equal("=", tokens[9]);
            Assert.Equal("   Includes space  ", tokens[10]);
            Assert.Equal(11, tokens.Count);

            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, NoValue=").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Missing'Operator'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Missing='Operator', InSecond'Field'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Unexpected=End,").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Unexpected=End,").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed='test', Invalid='Quoting").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed='test', Invalid='Quoting\'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=""test"", Invalid='Quoting""").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"WithoutValue=     ").Tokenize());
            Assert.DoesNotThrow(() => new Tokenizer(@"WithoutValue='        '").Tokenize());
        }

        [Fact]
        public void Parser()
        {
            FilterParser parser;
            IFilter result;
            FilterAndSet filters;

            parser = new FilterParser("FirstField=Value, SecondField=OtherValue | OrSecond = SomethingElse | OrThird ~= Field, Last!=Condition");
            result = parser.Parse();

            Assert.True(result is FilterAndSet);
            filters = (FilterAndSet)result;
            Assert.Equal(3, ((FilterAndSet)filters).Count);

            Assert.IsType<Filter>(filters[0]);
            Assert.Equal("FirstField", (filters[0] as Filter).Field);
            Assert.Equal("=", (filters[0] as Filter).Operator);
            Assert.Equal("Value", (filters[0] as Filter).Value);

            Assert.IsType<FilterOrSet>(filters[1]);
            Assert.Equal(3, (filters[1] as FilterOrSet).Count);
            (filters[1] as FilterOrSet).ForEach(x => Assert.IsType<Filter>(x));
            Assert.Equal("SecondField", ((filters[1] as FilterOrSet)[0] as Filter).Field);
            Assert.Equal("=", ((filters[1] as FilterOrSet)[0] as Filter).Operator);
            Assert.Equal("OtherValue", ((filters[1] as FilterOrSet)[0] as Filter).Value);
            Assert.Equal("OrSecond", ((filters[1] as FilterOrSet)[1] as Filter).Field);
            Assert.Equal("=", ((filters[1] as FilterOrSet)[1] as Filter).Operator);
            Assert.Equal("SomethingElse", ((filters[1] as FilterOrSet)[1] as Filter).Value);
            Assert.Equal("OrThird", ((filters[1] as FilterOrSet)[2] as Filter).Field);
            Assert.Equal("~=", ((filters[1] as FilterOrSet)[2] as Filter).Operator);
            Assert.Equal("Field", ((filters[1] as FilterOrSet)[2] as Filter).Value);

            Assert.IsType<Filter>(filters[2]);
            Assert.Equal("Last", (filters[2] as Filter).Field);
            Assert.Equal("!=", (filters[2] as Filter).Operator);
            Assert.Equal("Condition", (filters[2] as Filter).Value);
        }

        [Fact]
        public void Filter()
        {
            Assert.True(TestAgainstFilter("FirstField == EqualsValue", new { FirstField = "EqualsValue" }));
            Assert.False(TestAgainstFilter("FirstField = OtherValue", new { FirstField = "EqualsValue" }));
            Assert.True(TestAgainstFilter("Field ^= 'Begin  '", new { Field = "Begin  with some spaces" }));
            Assert.True(TestAgainstFilter("Field ~= CaSeInSensitivE", new { Field = "caseinsensITIVe" }));
            Assert.True(TestAgainstFilter("Field *= text", new { Field = "some text" }));
            
            Assert.False(TestAgainstFilter("Field > 10", new { Field = 1 }));
            Assert.True(TestAgainstFilter("Field < 8", new { Field = 1 }));
            Assert.True(TestAgainstFilter("Field <= 8", new { Field = 8 }));

            Assert.True(TestAgainstFilter("Field > 1234567890", new { Field = 1234567891L }));
            Assert.False(TestAgainstFilter("Field != 800", new { Field = 800L }));

            Assert.True(TestAgainstFilter("Field *= hello", new { Field = new List<string>() { "test", "hello" } }));
            Assert.False(TestAgainstFilter("Field *= hello", new { Field = new List<string>() { "else", "text" } }));
            Assert.True(TestAgainstFilter("Field *= 8", new { Field = new List<int>() { 5, 7, 8 } }));
        }

        private bool TestAgainstFilter(string filterText, object obj)
        {
            FilterParser parser = new FilterParser(filterText);
            IFilter filter = parser.Parse();

            filter.ExpectType(obj.GetType());
            return filter.Matches(obj);
        }
    }
}
