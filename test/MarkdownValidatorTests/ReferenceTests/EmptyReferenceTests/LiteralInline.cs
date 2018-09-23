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

namespace MihaZupan.MarkdownValidator.Tests.ReferenceTests.EmptyReferenceTests
{
    public class LiteralInline
    {
        [Fact]
        public void Single_Solo()
        {
            string source = @"[]";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.EmptyReference, 1, 0, 1, string.Empty));
        }

        [Fact]
        public void Single_InText()
        {
            string source = @"text[]text";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.EmptyReference, 1, 4, 5, string.Empty));
        }

        [Fact]
        public void Single_LineTest()
        {
            string source = @"
[]
text[]text
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.EmptyReference, 2, 1, 2, string.Empty),
                (WarningIDs.EmptyReference, 3, 8, 9, string.Empty));
        }

        [Fact]
        public void Double_Solo()
        {
            string source = @"[test][]";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.UnresolvedReference, 1, 0, 5, "test"),
                (WarningIDs.EmptyReference, 1, 6, 7, string.Empty));
        }

        [Fact]
        public void Double_InText()
        {
            string source = @"text[test][]text";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.UnresolvedReference, 1, 4, 9, "test"),
                (WarningIDs.EmptyReference, 1, 10, 11, string.Empty));
        }

        [Fact]
        public void Double_LineTest()
        {
            string source = @"
[test][]
text[stuff][]text
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.UnresolvedReference, 2, 1, 6, "test"),
                (WarningIDs.UnresolvedReference, 3, 14, 20, "stuff"),
                (WarningIDs.EmptyReference, 2, 7, 8, string.Empty),
                (WarningIDs.EmptyReference, 3, 21, 22, string.Empty));
        }
    }
}
