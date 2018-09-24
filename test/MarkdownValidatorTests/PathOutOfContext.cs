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
            SingleFileTest.AssertWarning(source,
                WarningIDs.UnresolvedReference, 0, 14, "foo/bar");
        }

        [Fact]
        public void OutOfContext()
        {
            // PathNotInContext maps to the actual reference path 
            string source = @"[text](../foo.bar)";
            SingleFileTest.AssertWarning(source,
                WarningIDs.PathNotInContext, 7, 16, "../foo.bar");
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
                (WarningIDs.PathNotInContext, 8, 17, "../foo.bar"),
                (WarningIDs.PathNotInContext, 28, 37, "../bar.foo"));
        }
    }
}
