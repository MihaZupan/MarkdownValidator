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
    public class UnresolvableHostname
    {
        [Fact]
        public void Unresolvable()
        {
            string url = "https://httpbin.notorg/";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.UnresolvableHostname, 6, 6 + url.Length - 1, url);
        }

        [Fact]
        public void UnresolvableAfterRedirect()
        {
            string url = $"{Constants.TEST_HOST}/redirect-to?url=https%3A%2F%2Fhttpbin.notorg";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.UnresolvableHostname, 6, 6 + url.Length - 1, url);
        }
    }
}
