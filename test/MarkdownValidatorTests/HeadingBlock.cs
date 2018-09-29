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

namespace MihaZupan.MarkdownValidator.Tests
{
    public class HeadingBlock
    {
        [Fact]
        public void ReferenceInHeading()
        {
            // References defined on heading lines are ignored
            string source = @"# Foo Hello [World] Bar";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void Empty_Single()
        {
            string source = @"#";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyHeading, 0, 0, source);
        }

        [Fact]
        public void Empty_Multiple()
        {
            string source = "###";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyHeading, 0, 2, source);
        }

        [Fact]
        public void Empty_Indented()
        {
            string source = "  ###";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyHeading, 2, 4, source.TrimStart());
        }

        [Fact]
        public void Empty_Multiple_TrailingWhitespace()
        {
            string source = "###  ";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EmptyHeading, 0, 4, source);
        }

        [Fact]
        public void EffectivelyEmpty_Single()
        {
            string source = @"# --";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EffectivelyEmptyHeading, 0, 3, source);
        }

        [Fact]
        public void EffectivelyEmpty_Multiple()
        {
            string source = @" ### -!";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EffectivelyEmptyHeading, 1, 6, "### -!");
        }

        [Fact]
        public void EndsInWhitespace_Single()
        {
            string source = @"# text ";
            SingleFileTest.AssertWarning(source,
                WarningIDs.HeadingEndsWithWhitespace, 0, 6, source);
        }

        [Fact]
        public void EndsInWhitespace_Multiple()
        {
            string source = @"### text ";
            SingleFileTest.AssertWarning(source,
                WarningIDs.HeadingEndsWithWhitespace, 0, 8, source);
        }

        [Fact]
        public void EndsInWhitespace_Indented()
        {
            string source = @"  ### text ";
            SingleFileTest.AssertWarning(source,
                WarningIDs.HeadingEndsWithWhitespace, 2, 10, source.TrimStart());
        }

        [Fact]
        public void EndsInWhitespace_LineTest()
        {
            string source = @"
  ### text 
# text2 
   ##  stuff  
";
            SingleFileTest.AssertWarnings(source,
                (WarningIDs.HeadingEndsWithWhitespace, 3, 11, "### text "),
                (WarningIDs.HeadingEndsWithWhitespace, 13, 20, "# text2 "),
                (WarningIDs.HeadingEndsWithWhitespace, 25, 35, "##  stuff  "));
        }
    }
}
