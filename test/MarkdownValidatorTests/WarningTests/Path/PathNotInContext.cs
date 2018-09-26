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

namespace MihaZupan.MarkdownValidator.Tests.WarningTests.Path
{
    public class PathNotInContext
    {
        [Fact]
        public void NotInContext()
        {
            string source = @"[Foo](../bar)";
            SingleFileTest.AssertWarning(source,
                WarningIDs.PathNotInContext, 6, 11, "../bar");
        }

        [Fact]
        public void NotInContext_LineTest()
        {
            string source = @"
[Foo](../bar)
";
            SingleFileTest.AssertWarning(source,
                WarningIDs.PathNotInContext, 7, 12, "../bar");
        }
    }
}
