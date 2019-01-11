/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
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
    public class Redirects
    {
        [Fact]
        [Trait("Category", "Network")]
        public void NoRedirects()
        {
            string source = $"[Foo]({Constants.TEST_HOST})";
            SingleFileTest.AssertNotPresent(source, WarningIDs.RedirectChain);
        }

        [Fact]
        [Trait("Category", "Network")]
        public void Redirect()
        {
            string url = $"{Constants.TEST_HOST}/redirect/1";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.RedirectChain, 6, 6 + url.Length - 1, url);
        }

        [Fact]
        [Trait("Category", "Network")]
        public void TooManyRedirects()
        {
            string url = $"{Constants.TEST_HOST}/redirect/10";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.TooManyRedirects, 6, 6 + url.Length - 1, url);
        }
    }
}
