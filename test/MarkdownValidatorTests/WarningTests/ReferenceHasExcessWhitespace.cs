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

namespace MihaZupan.MarkdownValidator.Tests.WarningTests
{
    public class ReferenceHasExcessWhitespace
    {
        [Fact]
        public void LinkInline_Appended()
        {
            string source = @"[Foo](. )";
            SingleFileTest.AssertWarning(source,
                WarningIDs.ReferenceHasExcessWhitespace, 0, 8, string.Empty);
        }

        [Fact]
        public void LinkInline_Prepended()
        {
            string source = @"[Foo]( .)";
            SingleFileTest.AssertWarning(source,
                WarningIDs.ReferenceHasExcessWhitespace, 0, 8, string.Empty);
        }

        [Fact]
        public void LinkInline_BothSides()
        {
            string source = @"[Foo]( . )";
            SingleFileTest.AssertWarning(source,
                WarningIDs.ReferenceHasExcessWhitespace, 0, 9, string.Empty);
        }

        // Note that LinkInlineParser emits warnings for the whole span
        // where as the LiteralInlineParser does them for specific parts

        [Fact]
        public void Single_Appended()
        {
            string source = @"[Foo ]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void Single_Prepended()
        {
            string source = @"[ Foo]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void Single_BothSides()
        {
            string source = @"[ Foo ]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void LiteralInline_Appended()
        {
            string source = @"[Foo][bar ]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void LiteralInline_Prepended()
        {
            string source = @"[Foo][ bar]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void LiteralInline_BothSides()
        {
            string source = @"[Foo][ bar ]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void LiteralInline_Content_Appended()
        {
            string source = @"[Foo ][bar]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void LiteralInline_Content_Prepended()
        {
            string source = @"[ Foo][bar]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }

        [Fact]
        public void LiteralInline_Content_BothSides()
        {
            string source = @"[ Foo ][bar}";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceHasExcessWhitespace);
        }
    }
}
