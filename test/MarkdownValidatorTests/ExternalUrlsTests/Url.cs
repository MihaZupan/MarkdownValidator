/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Tests.Framework;
using MihaZupan.MarkdownValidator.Warnings;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.ExternalUrlsTests
{
    public class Url
    {
        [Fact]
        public void NoWarning()
        {
            SingleFileTest.AssertNoWarnings("http://test.com", false);
        }

        [Theory]
        [InlineData("[foo](:)")]
        [InlineData("[foo](://test.com)")]
        public void InvalidFormat(string value)
        {
            SingleFileTest.AssertContainsWarning(value, WarningIDs.InvalidUrlFormat, false);
        }

        [Theory]
        [InlineData("http://1.2.3.4/")]
        [InlineData("http://[::]/")]
        [InlineData("[foo](http://1.2.3.4/)")]
        [InlineData("[foo](http://[::]/)")]
        public void IPHostname(string value)
        {
            SingleFileTest.AssertContainsWarning(value, WarningIDs.UrlHostnameIsIP, false);
        }

        [Theory]
        [InlineData("http://something.example/")]
        [InlineData("www.example.com#")]
        [InlineData("[foo](ftp://example.com)")]
        [InlineData("[foo](http://www.example.org?a=b)")]
        [InlineData("[foo](http://test.example.net)")]
        [InlineData("[foo](http://198.51.100.55/)")]
        [InlineData("[foo]: http://[2001:DB8::]")]
        [InlineData("http://gugl/.../")]
        public void DocumentationReserved(string value)
        {
            SingleFileTest.AssertNotPresent(value,
                WarningIDs.InvalidUrlFormat,
                WarningIDs.UrlHostnameIsIP,
                WarningIDs.UnresolvedReference);
        }
    }
}
