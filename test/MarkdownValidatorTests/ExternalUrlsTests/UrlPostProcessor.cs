/*
    Copyright (c) 2019 Miha Zupan. All rights reserved.
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
    public class UrlPostProcessor
    {
        [Fact]
        [Trait("Category", "Network")]
        public void TelegramBotApiDocs_ValidFragment()
        {
            string source = $"[Foo](https://core.telegram.org/bots/api#getting-updates)";
            SingleFileTest.AssertNotPresent(source, WarningIDs.InvalidUrlFragment);
        }

        [Fact]
        [Trait("Category", "Network")]
        public void TelegramBotApiDocs_InvalidFragment()
        {
            string url = "https://core.telegram.org/bots/api#random-invalid-fragment";
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source, WarningIDs.InvalidUrlFragment, 6, 6 + url.Length - 1, url);
        }
    }
}
