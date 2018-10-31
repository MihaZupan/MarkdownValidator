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

namespace MihaZupan.MarkdownValidator.Tests.ReferenceTests.EmptyReferenceTests
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
            string source = @"[text]()";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyReference, 0, 7, string.Empty);
        }

        [Fact]
        public void InText()
        {
            string source = @"text[text]()text";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyReference, 4, 11, string.Empty);
        }

        [Fact]
        public void LineTest()
        {
            string source = @"
[text]()
text[text]()text
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.EmptyReference, 1, 8, string.Empty),
                (WarningIDs.EmptyReference, 14, 21, string.Empty));
        }
    }
}
