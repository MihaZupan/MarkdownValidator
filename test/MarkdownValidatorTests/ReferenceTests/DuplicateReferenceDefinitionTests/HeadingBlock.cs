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
    public class HeadingBlock
    {
        [Fact]
        public void NoWarning_SingleHeading()
        {
            string source = @"# test";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void NoWarning_DifferentHeadings()
        {
            string source = @"
# test
# test 2
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void Single_SameLevel()
        {
            string source = @"
# title
# title";
            SingleFileTest.AssertWarnings(source,
                (WarningID.DuplicateHeadingDefinition, 3, 11, 17, "# title"));
        }

        [Fact]
        public void Single_DifferentLevels()
        {
            string source = @"
# title
## title";
            SingleFileTest.AssertWarnings(source,
                (WarningID.DuplicateHeadingDefinition, 3, 11, 18, "## title"));
        }

        [Fact]
        public void Multiple()
        {
            string source = @"
# title
## test 2
### title
#### test2
##### test 2
";
            SingleFileTest.AssertWarnings(source,
                (WarningID.DuplicateHeadingDefinition, 4, 22, 30, "### title"),
                (WarningID.DuplicateHeadingDefinition, 6, 45, 56, "##### test 2"));
        }
    }
}
