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
