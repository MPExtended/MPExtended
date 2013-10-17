﻿#region Copyright (C) 2012-2013 MPExtended
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
            string[] expectedTokens;

            tokenizer = new Tokenizer(@"Field=Value,AnotherField    $= 'AnotherValue'| NumericField> 5, DoubleQuoted ^= ""double quoted""");
            expectedTokens = new string[] { "Field", "=", "Value", ",", "AnotherField", "$=", "AnotherValue", "|", "NumericField", ">", "5", ",",
                                            "DoubleQuoted", "^=", "double quoted" };
            AssertTokenizer(tokenizer, expectedTokens);

            tokenizer = new Tokenizer(@"EscapedField='hoi\'test\\more'    ,ComplicatedField=""test\\\"""" | withoutQuotes=hello\,more");
            expectedTokens = new string[] { "EscapedField", "=", @"hoi'test\more", ",", "ComplicatedField", "=", @"test\""", "|",
                                            "withoutQuotes", "=", "hello,more" };
            AssertTokenizer(tokenizer, expectedTokens);

            tokenizer = new Tokenizer(@"Field=Name With Whitespace,   AnotherField ^=  Prefixed and Suffixed space   | QuotedField='   Includes space  '");
            expectedTokens = new string[] { "Field", "=", "Name With Whitespace", ",", "AnotherField", "^=", "Prefixed and Suffixed space", "|", 
                                            "QuotedField", "=", "   Includes space  " };
            AssertTokenizer(tokenizer, expectedTokens);

            tokenizer = new Tokenizer(@"SomeField    = Contains A Value, FirstList = [ 'hello',another     , more values ], PipeList*=[xyz|abc def|ghi|'quot|ed']");
            expectedTokens = new string[] { "SomeField", "=", "Contains A Value", ",", "FirstList", "=", "[", "hello", ",", "another", ",", "more values", "]", ",",
                                            "PipeList", "*=", "[", "xyz", "|", "abc def", "|", "ghi", "|", "quot|ed", "]" };
            AssertTokenizer(tokenizer, expectedTokens);

            tokenizer = new Tokenizer(@"Field = [Item1, Item2, Item3] | SecondField = [Item4 | Item5], ThirdField = [Item6]");
            expectedTokens = new string[] { "Field", "=", "[", "Item1", ",", "Item2", ",", "Item3", "]", "|", "SecondField", "=", "[", "Item4", "|", "Item5", "]",
                                            ",", "ThirdField", "=", "Item6", "]" };

            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, NoValue=").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Missing'Operator'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Missing='Operator', InSecond'Field'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Unexpected=End,").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=test, Unexpected=End,").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed='test', Invalid='Quoting").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed='test', Invalid='Quoting\'").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Seed=""test"", Invalid='Quoting""").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"Field=[Invalid|Mixing,Conjunctions]").Tokenize());
            Assert.Throws<ParseException>(() => new Tokenizer(@"WithoutValue=     ").Tokenize());
            Assert.DoesNotThrow(() => new Tokenizer(@"WithoutValue='        '").Tokenize());
        }

        private void AssertTokenizer(Tokenizer tokenizer, string[] expected)
        {
            var tokens = tokenizer.Tokenize();
            int i = 0;
            foreach (var expectedItem in expected)
                Assert.Equal(expectedItem, tokens[i++]);
            Assert.Equal(i, tokens.Count);
        }

        [Fact]
        public void Parser()
        {
            FilterParser parser;
            IFilter result;
            FilterAndSet filters;

            parser = new FilterParser("FirstField=Value, SecondField=OtherValue | OrSecond = SomethingElse | OrThird ~= Field, Last*=[Hello|Bye]");
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

            Assert.IsType<ListFilter>(filters[2]);
            Assert.Equal("Last", (filters[2] as ListFilter).Field);
            Assert.Equal("*=", (filters[2] as ListFilter).Operator);
            Assert.Equal(2, (filters[2] as ListFilter).Values.Length);
            Assert.Equal(ListFilter.JoinType.Or, (filters[2] as ListFilter).Type);
            Assert.Equal("Hello", (filters[2] as ListFilter).Values[0]);
            Assert.Equal("Bye", (filters[2] as ListFilter).Values[1]);
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

            Assert.True(TestAgainstFilter("Field = [3, 8]", new { Field = 3 }));
            Assert.True(TestAgainstFilter("Field = [3, 8]", new { Field = 8 }));
            Assert.False(TestAgainstFilter("Field = [3, 8]", new { Field = 9L }));
            Assert.False(TestAgainstFilter("Field != [3, 8]", new { Field = 3L }));

            Assert.True(TestAgainstFilter("Field = true", new { Field = true }));
            Assert.False(TestAgainstFilter("Field == false", new { Field = true }));
            Assert.True(TestAgainstFilter("Field != 0", new { Field = true }));
            Assert.True(TestAgainstFilter("Field != 1", new { Field = false }));

            Assert.True(TestAgainstFilter("Field ~= [abc, def, ghi]", new { Field = "abc" }));
            Assert.False(TestAgainstFilter("Field *= [klm, nop]", new { Field = "hello" }));
            Assert.False(TestAgainstFilter("Field ^= [qrst]", new { Field = "uvw" }));
            Assert.True(TestAgainstFilter("Field $= [uvw, xyz]", new { Field = "abcxyz" }));

            Assert.True(TestAgainstFilter("Field *= [efgh, hijk]", new { Field = new string[] { "abcd", "efgh", "hijk", "lmno" } }));
            Assert.True(TestAgainstFilter("Field *= [efgh | hijk]", new { Field = new string[] { "abcd", "efgh" } }));
            Assert.False(TestAgainstFilter("Field *= [abcd, efgh]", new { Field = new string[] { "abcd", "hijk" } }));
            Assert.False(TestAgainstFilter("Field *= [abcd| efgh]", new { Field = new string[] { "hijk", "lmno", "pqrs" }}));

            Assert.True(TestAgainstFilter("Field == [abcd, efgh]", new { Field = new string[] { "abcd", "efgh" } }));
            Assert.False(TestAgainstFilter("Field == [abcd, efgh]", new { Field = new string[] { "abcd", "efgh", "ijkl" } }));
            Assert.False(TestAgainstFilter("Field == [abcd, efgh, ijkl]", new { Field = new string[] { "abcd", "efgh" } }));

            Assert.False(TestAgainstFilter("Field != [abcd, efgh]", new { Field = new string[] { "abcd", "efgh" } }));
            Assert.True(TestAgainstFilter("Field != [abcd, efgh]", new { Field = new string[] { "abcd", "efgh", "ijkl" } }));
            Assert.True(TestAgainstFilter("Field != [abcd, efgh, ijkl]", new { Field = new string[] { "abcd", "efgh" } }));

            Assert.Throws<ArgumentException>(() => TestAgainstFilter("Field != 2", new { Field = true }));
            Assert.Throws<ArgumentException>(() => TestAgainstFilter("Field != abc", new { Field = 3 }));
            Assert.Throws<ArgumentException>(() => TestAgainstFilter("Field != abc", new { Field = 8L }));
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
