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
    public class WebRequestTimedOut
    {
        [Fact]
        [Trait("Category", "Network")]
        public void NoTimeout()
        {
            string source = $"[Foo]({Constants.TEST_HOST})";
            SingleFileTest.AssertNotPresent(source, WarningIDs.WebRequestTimedOut);
        }

        [Fact]
        [Trait("Category", "Network")]
        public void Timeout()
        {
            string url = $"{Constants.TEST_HOST}/delay/4";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.WebRequestTimedOut, 6, 6 + url.Length - 1, url);
        }

        [Fact]
        [Trait("Category", "Network")]
        public void TimeoutAfterRedirect()
        {
            string url = $"{Constants.TEST_HOST}/redirect-to?url={Constants.TEST_HOST_ENCODED}%2Fdelay%2F4";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.WebRequestTimedOut, 6, 6 + url.Length - 1, url);
        }
    }
}
