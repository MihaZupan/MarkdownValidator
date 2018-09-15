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
    public class PathOutOfContext
    {
        [Fact]
        public void Unresolved()
        {
            // UnresolvedReference maps to the entire reference
            string source = @"[text](foo/bar)";
            SingleFileTest.AssertWarnings(source,
                (WarningID.UnresolvedReference, 1, 0, 14, "foo/bar"));
        }

        [Fact]
        public void OutOfContext()
        {
            // PathNotInContext maps to the actual reference path 
            string source = @"[text](../foo.bar)";
            SingleFileTest.AssertWarnings(source,
                (WarningID.PathNotInContext, 1, 7, 16, "../foo.bar"));
        }

        [Fact]
        public void OutOfContext_LineTest()
        {
            // PathNotInContext maps to the actual reference path 
            string source = @"
[text](../foo.bar)
[stuff](../bar.foo)
";
            SingleFileTest.AssertWarnings(source,
                (WarningID.PathNotInContext, 2, 8, 17, "../foo.bar"),
                (WarningID.PathNotInContext, 3, 28, 37, "../bar.foo"));
        }
    }
}
