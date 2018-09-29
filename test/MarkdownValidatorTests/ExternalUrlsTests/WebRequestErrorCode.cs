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
    [Trait("Category", "WebIO")]
    public class WebRequestErrorCode
    {
        [Theory]
        [InlineData("https://httpbin.org/status/100")]
        [InlineData("https://httpbin.org/status/400")]
        [InlineData("https://httpbin.org/status/500")]
        [InlineData("https://httpbin.org/status/504")]
        public void ErrorCode(string url)
        {
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.WebRequestReturnedErrorCode, 6, 6 + url.Length - 1, url);
        }

        [Theory]
        [InlineData("https://httpbin.org/redirect-to?url=https%3A%2F%2Fhttpbin.org%2Fstatus%2F100")]
        [InlineData("https://httpbin.org/redirect-to?url=https%3A%2F%2Fhttpbin.org%2Fstatus%2F400")]
        [InlineData("https://httpbin.org/redirect-to?url=https%3A%2F%2Fhttpbin.org%2Fstatus%2F500")]
        [InlineData("https://httpbin.org/redirect-to?url=https%3A%2F%2Fhttpbin.org%2Fstatus%2F504")]
        public void ErrorCodeAfterRedirect(string url)
        {
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.WebRequestReturnedErrorCode, 6, 6 + url.Length - 1, url);
        }
    }
}
