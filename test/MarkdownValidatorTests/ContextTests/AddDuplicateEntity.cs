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
    public class AddDuplicateEntity
    {
        [Fact]
        public void DuplicateMarkdownFile()
        {
            var test = new RollingContextTest()
                .Update("[Some error]")
                .AssertContains(WarningIDs.UnresolvedReference)
                .AddMarkdownFile("[Not an error anymore](.)", assertReturn: false)
                .AssertContains(WarningIDs.UnresolvedReference);
        }

        [Fact]
        public void DuplicateEntity()
        {
            var test = new RollingContextTest()
                .AddEntity("foo.bar")
                .Update("[Foo](foo.bar)")
                .AssertNoWarnings()
                .Update("[Foo](Foo.bar)")
                .AssertContains(WarningIDs.FileReferenceCaseMismatch)
                .AddEntity("Foo.bar", assertReturn: false)
                .AssertContains(WarningIDs.FileReferenceCaseMismatch);
        }
    }
}