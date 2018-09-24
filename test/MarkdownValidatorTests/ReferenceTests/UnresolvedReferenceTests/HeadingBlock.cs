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
    public class HeadingBlock
    {
        [Fact]
        public void NoWarning_InlineLink()
        {
            string source = @"
# text
[stuff](#text)
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void NoWarning_LinkReference()
        {
            string source = @"
[stuff]
# text

[stuff]: #text
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void UnresolvedHeadingReference_InlineLink()
        {
            string source = @"[stuff](#text)";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.UnresolvedReference, 0, 13, "#text"));
        }

        [Fact]
        public void UnresolvedHeadingReference_LinkReference()
        {
            string source = @"
[foo][stuff]

[stuff]: #text
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.UnresolvedReference, 15, 28, "#text"));
        }

        [Fact]
        public void UnresolvedHeadingReference_DifferentLevels()
        {
            string source = @"
## text
[stuff](##text)
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.UnresolvedReference, 9, 23, "##text"));
        }
    }
}
