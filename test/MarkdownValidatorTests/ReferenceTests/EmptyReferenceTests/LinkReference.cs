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
    public class LinkReference
    {
        [Fact]
        public void Solo()
        {
            string source = @"[]: .";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyReference, 1, 0, 1, string.Empty));
        }

        [Fact]
        public void InText()
        {
            string source = @"
text

[]: .
text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyReference, 4, 10, 11, string.Empty));
        }

        [Fact]
        public void LineTest()
        {
            string source = @"
[]: .
text

[]: .
text";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyReference, 2, 2, 3, string.Empty),
                (WarningID.EmptyReference, 5, 17, 18, string.Empty));
        }
    }
}
