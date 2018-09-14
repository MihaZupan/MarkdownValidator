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
        public void Empty_Single()
        {
            string source = @"#";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyHeading, 1, 0, 0, source));
        }

        [Fact]
        public void Empty_Multiple()
        {
            string source = "###";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyHeading, 1, 0, 2, source));
        }

        [Fact]
        public void Empty_Indented()
        {
            string source = "  ###";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyHeading, 1, 2, 4, source.TrimStart()));
        }

        [Fact]
        public void Empty_Multiple_TrailingWhitespace()
        {
            string source = "###  ";
            SingleFileTest.AssertWarnings(source,
                (WarningID.EmptyHeading, 1, 0, 4, source));
        }

        [Fact]
        public void EndsInWhitespace_Single()
        {
            string source = @"# text ";
            SingleFileTest.AssertWarnings(source,
                (WarningID.HeadingEndsWithWhitespace, 1, 0, 6, source));
        }

        [Fact]
        public void EndsInWhitespace_Multiple()
        {
            string source = @"### text ";
            SingleFileTest.AssertWarnings(source,
                (WarningID.HeadingEndsWithWhitespace, 1, 0, 8, source));
        }

        [Fact]
        public void EndsInWhitespace_Indented()
        {
            string source = @"  ### text ";
            SingleFileTest.AssertWarnings(source,
                (WarningID.HeadingEndsWithWhitespace, 1, 2, 10, source.TrimStart()));
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
                (WarningID.HeadingEndsWithWhitespace, 2, 4, 12, "### text "),
                (WarningID.HeadingEndsWithWhitespace, 3, 15, 22, "# text2 "),
                (WarningID.HeadingEndsWithWhitespace, 4, 28, 38, "##  stuff  "));
        }
    }
}
