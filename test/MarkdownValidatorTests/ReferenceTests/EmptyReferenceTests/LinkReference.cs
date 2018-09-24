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
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyReference, 0, 1, string.Empty);
        }

        [Fact]
        public void InText()
        {
            string source = @"
text

[]: .
text
";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyReference, 7, 8, string.Empty);
        }

        [Fact]
        public void LineTest()
        {
            string source = @"
[]: .
text

[]: .
text
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.EmptyReference, 1, 2, string.Empty),
                (WarningIDs.EmptyReference, 13, 14, string.Empty));
        }
    }
}
