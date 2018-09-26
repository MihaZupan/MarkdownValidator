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

namespace MihaZupan.MarkdownValidator.Tests.WarningTests.Path
{
    public class FileReferenceCaseMismatch
    {
        [Fact]
        public void Test()
        {
            var test = new RollingContextTest(new Config(""))
                .AddEntity("Foo.bar")
                .Update("[Stuff](foo.bar)")
                .AssertContains(WarningIDs.FileReferenceCaseMismatch, 8, 14, "foo.bar")
                .AssertContains(WarningIDs.UnresolvedReference)
                .Clear()
                .AssertNoWarnings()
                .Update("[Test](bar.Foo)")
                .AddEntity("bar.foo")
                .AssertContains(WarningIDs.FileReferenceCaseMismatch, 7, 13, "bar.Foo")
                .AssertWarningCount(2);
        }
    }
}
