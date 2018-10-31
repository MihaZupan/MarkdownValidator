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

namespace MihaZupan.MarkdownValidator.Tests.WarningTests
{
    public class ReferenceContainsLineBreak
    {
        [Fact]
        public void LiteralInline_Content()
        {
            string source = "[F\noo][bar]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceContainsLineBreak);
        }

        [Fact]
        public void LiteralInline_Reference()
        {
            string source = "[Foo][ba\nr]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceContainsLineBreak);
        }

        [Fact]
        public void LiteralInline_Both()
        {
            string source = "[\nFoo][ba\nr]";
            SingleFileTest.AssertContainsWarning(source, WarningIDs.ReferenceContainsLineBreak);
        }
    }
}
