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
    public class LinkInline
    {
        [Fact]
        public void NoWarning()
        {
            string source = @"[text](.)";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void Solo()
        {
            string source = @"[](.)";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyLinkContent, 1, 0, 4, string.Empty));
        }

        [Fact]
        public void InText()
        {
            string source = @"text[](.)text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyLinkContent, 1, 4, 8, string.Empty));
        }

        [Fact]
        public void LineTest()
        {
            string source = @"
[](.)
text[](.)text
";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyLinkContent, 2, 1, 5, string.Empty),
                (WarningID.EmptyLinkContent, 3, 11, 15, string.Empty));
        }
    }
}
