/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Tests.Framework;
using System;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.ExternalUrlsTests
{
    public class MultipleSame
    {
        [Fact]
        public void MultipleSameHostname()
        {
            string source = @"
[Stuff] [Things]

[Foo](TEST_HOST)
[Bar](TEST_HOST/spec.json)

[Stuff]: TEST_HOST/cookies
[Things]: TEST_HOST/forms/post#123
".Replace("TEST_HOST", Constants.TEST_HOST, StringComparison.Ordinal);
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void MultipleSameUrl()
        {
            string source = @"
[Stuff] [Things]

[Foo](TEST_HOST/forms/post)
[Bar](TEST_HOST/forms/post)

[Stuff]: TEST_HOST/forms/post#123
[Things]: TEST_HOST/forms/post
".Replace("TEST_HOST", Constants.TEST_HOST, StringComparison.Ordinal);
            SingleFileTest.AssertNoWarnings(source);
        }
    }
}
