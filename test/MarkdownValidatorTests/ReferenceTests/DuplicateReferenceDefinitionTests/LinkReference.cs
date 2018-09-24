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

namespace MihaZupan.MarkdownValidator.Tests.DuplicateReferenceDefinitionTests
{
    public class LinkReference
    {
        [Fact]
        public void NoWarning_Single()
        {
            string source = @"
[test]

[test]: .
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void NoWarning_Multiple()
        {
            string source = @"
[test]
[stuff]
[test]

[test]: .
[stuff]: .
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void Duplicated_Single()
        {
            string source = @"
[test]

[test]: .
[test]: .
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.DuplicateReferenceDefinition, 19, 27, "test"));
        }

        [Fact]
        public void Duplicated_Multiple()
        {
            string source = @"
[stuff]
[test]

[test]: .
[stuff]: .
[stuff]: .
[test]: .
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.DuplicateReferenceDefinition, 38, 47, "stuff"),
                (WarningIDs.DuplicateReferenceDefinition, 49, 57, "test"));
        }
    }
}
