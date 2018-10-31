/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Tests.Framework;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.ContextTests
{
    public class RemoveEntity
    {
        [Fact]
        public void RemoveNonMarkdownFile()
        {
            var test = new RollingContextTest()
                .AddEntity("foo.bar")
                .Update("[Foo](foo.bar)")
                .AssertNoWarnings()
                .RemoveEntity("foo.bar", true)
                .AssertHasWarnings();
        }

        [Fact]
        public void RemoveNonExistantEntity()
        {
            var test = new RollingContextTest()
                .RemoveEntity("foo.bar", false);
        }
    }
}