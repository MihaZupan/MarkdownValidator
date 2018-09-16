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

namespace MihaZupan.MarkdownValidator.Tests.ContextTests
{
    public class Test1
    {
        [Fact]
        public void TestContext()
        {
            var test = new RollingContextTest()
                .Update("Foo bar")
                .AssertNoWarnings()
                .Update("[]")
                .Assert(WarningID.EmptyReference, 1, 0, 1, string.Empty)
                .Update("[Foo](.)")
                .AssertNoWarnings()
                .Update("[Foo]()")
                .AssertContains(WarningID.EmptyReference)
                .AssertNotPresent(WarningID.UnresolvedReference)
                .Update("# Test", "Foo.md")
                .Update("[Bar](foo.md#test)")
                .AssertNoWarnings()
                .Update("\n[Bar](foo.md#tes)")
                // ToDo: report accurate LinkReferenceDefinition locations (6, 16)
                .Assert(WarningID.UnresolvedReference, 2, 1, 17, "foo.md#tes")
                .Update("[Bar]: foo.md#test")
                .Assert(WarningID.UnusedDefinedReference, 1, 0, 17, "Bar")
                .RemoveEntity("fOo.mD")
                .AssertContains(WarningID.UnresolvedReference, 1, 0, 17, "foo.md#test")
                .Clear()
                .AssertContains(WarningID.EmptyContext)
                .Update("\n[]()\n\n# Foo \n")
                .Assert(
                    // Need a change in Markdig to get accurate LinkInline source spans for Label and Url
                    (WarningID.EmptyLinkContent, 2, 1, 4, string.Empty),
                    (WarningID.EmptyReference, 2, 1, 4, string.Empty),
                    (WarningID.HeadingEndsWithWhitespace, 4, 7, 12, "# Foo "))
                .Clear()
                .Update("[Test]", "Bar.md")
                .Assert(WarningID.UnresolvedReference, 1, 0, 5, "Test", "Bar.md")
                .AssertContains(w => w.IsError)
                .AssertSingle(w => w.Message.Length > 5);
        }
    }
}
