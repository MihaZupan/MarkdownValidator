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

namespace MihaZupan.MarkdownValidator.Tests.ContextTests
{
    public class RemoveReference
    {
        [Fact]
        public void RemoveFileWithHeaderReference()
        {
            var test = new RollingContextTest()
                .Update("# Foo", "bar.md")
                .Update("[Foo](bar.md#foo)", "foo.md")
                .AssertNoWarnings()
                .RemoveEntity("foo.md")
                .Update("[Foo](bar.md#foo)", "foo.md")
                .Update("[Foo](bar.md#foo)", "foo2.md")                
                .RemoveEntity("foo.md")
                .RemoveEntity("bar.md")
                .AssertWarningCount(1)
                .Assert(WarningIDs.UnresolvedReference, 6, 15, "bar.md#foo", "foo2.md");
        }

        [Fact]
        public void RemoveHeaderReference()
        {
            var test = new RollingContextTest()
                .Update("# Foo\n# Bar", "test.md")
                .Update("[Bar](test.md#bar) [Foo](test.md#foo)")
                .AssertNoWarnings()
                .Update("# Bar", "test.md")
                .Assert(WarningIDs.UnresolvedReference, 25, 35, "test.md#foo");
        }
    }
}