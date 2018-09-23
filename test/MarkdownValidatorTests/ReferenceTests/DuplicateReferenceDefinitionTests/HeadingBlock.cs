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
# title
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.DuplicateHeadingDefinition, 3, 9, 15, "# title"));
        }

        [Fact]
        public void Single_DifferentLevels()
        {
            string source = @"
# title
## title
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.DuplicateHeadingDefinition, 3, 9, 16, "## title"));
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
                (WarningIDs.DuplicateHeadingDefinition, 4, 19, 27, "### title"),
                (WarningIDs.DuplicateHeadingDefinition, 6, 40, 51, "##### test 2"));
        }
    }
}
