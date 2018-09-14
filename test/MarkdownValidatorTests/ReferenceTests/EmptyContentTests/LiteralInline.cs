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

namespace MihaZupan.MarkdownValidator.Tests.ReferenceTests.EmptyContentTests
{
    public class LiteralInline
    {
        [Fact]
        public void Double()
        {
            string source = @"[][.]";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyLinkContent, 1, 0, 1, string.Empty));
        }

        [Fact]
        public void InText()
        {
            string source = @"text[][.]text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyLinkContent, 1, 4, 5, string.Empty));
        }

        [Fact]
        public void LineTest()
        {
            string source = @"
[][.]
text[][.]text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyLinkContent, 2, 2, 3, string.Empty),
                (WarningID.EmptyLinkContent, 3, 13, 14, string.Empty));
        }
    }
}
