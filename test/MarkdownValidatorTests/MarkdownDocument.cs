/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
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
            SingleFileTest.AssertGlobalWarning(source, WarningIDs.HugeMarkdownFile, $"{HugeLineCount} >= {HugeLineCount}");

            source = "Text" + new string('\n', HugeLineCount * 2);
            SingleFileTest.AssertGlobalWarning(source, WarningIDs.HugeMarkdownFile, $"{HugeLineCount * 2} >= {HugeLineCount}");
        }

        [Fact]
        public void EmptyFile()
        {
            SingleFileTest.AssertGlobalWarning(string.Empty, WarningIDs.EmptyMarkdownFile);
        }

        [Fact]
        public void EmptyFile_WhitespaceOnly()
        {
            SingleFileTest.AssertGlobalWarning(new string(' ', 5), WarningIDs.EmptyMarkdownFile);
        }

        [Fact]
        public void EmptyFile_NewLinesOnly()
        {
            SingleFileTest.AssertGlobalWarning(new string('\n', 5), WarningIDs.EmptyMarkdownFile);
        }

        [Fact]
        public void EmptyFile_WhiteSpaceAndNewLines()
        {
            SingleFileTest.AssertGlobalWarning(" \n \n \n ", WarningIDs.EmptyMarkdownFile);
        }
    }
}
