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

namespace MihaZupan.MarkdownValidator.Tests.ReferenceTests.UnusedReferenceTests
{
    public class LinkReference
    {
        [Fact]
        public void NoWarning_InlineLink()
        {
            string source = @"
[bar][foo]

[foo]: .
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void NoWarning_SameNameInlineLine()
        {
            string source = @"
[foo]

[foo]: .
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void UnusedLinkReference_Single()
        {
            string source = @"[foo]: .";
            SingleFileTest.AssertWarning(source,
                WarningIDs.UnusedDefinedReference, 0, 7, "foo");
        }

        [Fact]
        public void UnusedLinkReference_Mixed()
        {
            string source = @"
[foo]

[foo]: .
[bar]: .
[bar]

[stuff]: .
";
            SingleFileTest.AssertWarning(source,
                WarningIDs.UnusedDefinedReference, 33, 42, "stuff");
        }

        [Fact]
        public void UnusedLinkReference_Multiple()
        {
            string source = @"
[bar]

[foo]: .
[bar]: .
[stuff]: .
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.UnusedDefinedReference, 8, 15, "foo"),
                (WarningIDs.UnusedDefinedReference, 26, 35, "stuff"));
        }
    }
}
