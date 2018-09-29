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
    public class Redirects
    {
        [Fact]
        public void NoRedirects()
        {
            string source = "[Foo](https://www.google.com)";
            SingleFileTest.AssertNotPresent(source, WarningIDs.RedirectChain);
        }

        [Fact]
        public void Redirect()
        {
            string url = "https://httpbin.org/redirect/1";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.RedirectChain, 6, 6 + url.Length - 1, url);
        }

        [Fact]
        public void TooManyRedirects()
        {
            string url = "https://httpbin.org/redirect/10";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.TooManyRedirects, 6, 6 + url.Length - 1, url);
        }
    }
}
