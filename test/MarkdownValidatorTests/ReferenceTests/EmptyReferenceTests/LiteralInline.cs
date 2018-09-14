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
                (WarningID.EmptyReference, 1, 0, 1, string.Empty));
        }

        [Fact]
        public void Single_InText()
        {
            string source = @"text[]text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyReference, 1, 4, 5, string.Empty));
        }

        [Fact]
        public void Single_LineTest()
        {
            string source = @"
[]
text[]text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyReference, 2, 2, 3, string.Empty),
                (WarningID.EmptyReference, 3, 10, 11, string.Empty));
        }

        [Fact]
        public void Double_Solo()
        {
            string source = @"[test][]";
            SingleFileTest.AssertWarnings(source,
                (WarningID.UnresolvedReference, 1, 0, 5, "test"),
                (WarningID.EmptyReference, 1, 6, 7, string.Empty));
        }

        [Fact]
        public void Double_InText()
        {
            string source = @"text[test][]text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.UnresolvedReference, 1, 4, 9, "test"),
                (WarningID.EmptyReference, 1, 10, 11, string.Empty));
        }

        [Fact]
        public void Double_LineTest()
        {
            string source = @"
[test][]
text[stuff][]text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.UnresolvedReference, 2, 2, 7, "test"),
                (WarningID.UnresolvedReference, 3, 16, 22, "stuff"),
                (WarningID.EmptyReference, 2, 8, 9, string.Empty),
                (WarningID.EmptyReference, 3, 23, 24, string.Empty));
        }
    }
}
