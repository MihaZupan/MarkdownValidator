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

namespace MihaZupan.MarkdownValidator.Tests.ReferenceTests.UnusedReferenceTests
{
    public class Footnote
    {
        [Fact]
        public void UnusedFootnote()
        {
            string source = @"
Test[^1]

[^1]: Test
[^2]: Test 2
";
            SingleFileTest.AssertWarnings(source,
                (WarningID.UnusedDefinedFootnote, 5, 22, 25, "^2"));
        }
    }
}
