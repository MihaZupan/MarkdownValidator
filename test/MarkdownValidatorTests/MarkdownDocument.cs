/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Tests.Framework;
using MihaZupan.MarkdownValidator.Warnings;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests
{
    public class MarkdownDocument
    {
        private static readonly int HugeLineCount = new Config("Dir").Parsing.Warnings_HugeFile_LineCount;

        [Fact]
        public void LargeFile()
        {
            string source = "Text" + new string('\n', HugeLineCount - 1);
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void HugeFile()
        {
            string source = "Text" + new string('\n', HugeLineCount);
            AssertGlobalWarning(source, WarningID.HugeMarkdownFile, $"{HugeLineCount} >= {HugeLineCount}");

            source = "Text" + new string('\n', HugeLineCount * 2);
            AssertGlobalWarning(source, WarningID.HugeMarkdownFile, $"{HugeLineCount * 2} >= {HugeLineCount}");
        }

        [Fact]
        public void EmptyFile()
        {
            AssertGlobalWarning(string.Empty, WarningID.EmptyMarkdownFile);
        }

        [Fact]
        public void EmptyFile_WhitespaceOnly()
        {
            AssertGlobalWarning(new string(' ', 5), WarningID.EmptyMarkdownFile);
        }

        [Fact]
        public void EmptyFile_NewLinesOnly()
        {
            AssertGlobalWarning(new string('\n', 5), WarningID.EmptyMarkdownFile);
        }

        [Fact]
        public void EmptyFile_WhiteSpaceAndNewLines()
        {
            AssertGlobalWarning(" \n \n \n ", WarningID.EmptyMarkdownFile);
        }

        private void AssertGlobalWarning(string source, WarningID id, string value = "")
        {
            SingleFileTest.AssertWarnings(source,
                (id, 0, SourceSpan.Empty.Start, SourceSpan.Empty.End, value));
        }
    }
}
