#region Copyright (C) 2012-2013 MPExtended
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
using MPExtended.Libraries.Service.Util;
using Xunit;

namespace MPExtended.Tests.Libraries.Service.Util
{
    public class PathUtilTests
    {
        [Fact]
        public void StripInvalidCharacters()
        {
            Assert.Equal("valid_file.txt", PathUtil.StripInvalidCharacters("valid_file.txt"));
            Assert.Equal("FileWithoutExtension", PathUtil.StripInvalidCharacters("FileWithoutExtension"));
            Assert.Equal(@"directoryinvalidpath.txt", PathUtil.StripInvalidCharacters(@"directory\?invalid|path.txt"));
            Assert.Equal("in__aalidpath", PathUtil.StripInvalidCharacters("in<>aalidpath", "_"));
            Assert.Equal("more____", PathUtil.StripInvalidCharacters("more????", '_'));
            Assert.Equal("strip multimultimultinumbersmulti", PathUtil.StripInvalidCharacters("strip 123numbers1", new char[] { '0', '1', '2', '3' }, "multi"));
            Assert.Equal("&&re&strip", PathUtil.StripInvalidCharacters("more strip", new char[] { 'm', 'o', ' ' }, "&"));
        }

        [Fact]
        public void StripFileProtocolPrefix()
        {
            Assert.Equal(@"C:\test.txt", PathUtil.StripFileProtocolPrefix(@"C:\test.txt"));
            Assert.Equal(@"C:\this\path.txt", PathUtil.StripFileProtocolPrefix(@"file://C:\this\path.txt"));
            Assert.Equal(@"http://do/not/touch/this.txt", PathUtil.StripFileProtocolPrefix(@"http://do/not/touch/this.txt"));
        }

        [Fact]
        public void GetAbsolutePath()
        {
            Assert.Equal(@"C:\Parent\Child\file.txt", PathUtil.GetAbsolutePath(@"C:\Parent\Child", @"file.txt"));
            Assert.Equal(@"C:\Parent\in-parent.txt", PathUtil.GetAbsolutePath(@"C:\Parent\Child", @"..\in-parent.txt"));
            Assert.Equal(@"C:\Parent\OtherChild\file.txt", PathUtil.GetAbsolutePath(@"C:\Parent\Child", @"..\OtherChild\file.txt"));
        }
    }
}
