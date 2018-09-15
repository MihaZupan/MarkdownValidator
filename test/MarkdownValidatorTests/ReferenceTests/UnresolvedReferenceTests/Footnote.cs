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

namespace MihaZupan.MarkdownValidator.Tests.ReferenceTests.UnresolvedReferenceTests
{
    public class Footnote
    {
        [Fact]
        public void NoWarning()
        {
            string source = @"
Test[^1]

[^1]: Some text
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void MissingFootnoteDefinition()
        {
            string source = @"Test[^1]";
            SingleFileTest.AssertWarnings(source,
                (WarningID.UnresolvedFootnoteReference, 1, 4, 7, "^1"));
        }
    }
}
