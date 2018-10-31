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
    public class Test1
    {
        [Fact]
        public void TestContext()
        {
            var test = new RollingContextTest()
                .Update("Foo bar")
                .AssertNoWarnings()
                .Update("[]")
                .Assert(WarningIDs.EmptyReference, 0, 1, string.Empty)
                .Update("[Foo](.)")
                .AssertNoWarnings()
                .Update("[Foo]()")
                .AssertContains(WarningIDs.EmptyReference)
                .AssertNotPresent(WarningIDs.UnresolvedReference)
                .Update("# Test", "Foo.md")
                .Update("[Bar](foo.md#test)")
                .AssertNoWarnings()
                .Update("\n[Bar](foo.md#tes)")
                .Assert(WarningIDs.UnresolvedReference, 7, 16, "foo.md#tes")
                .Update("[Bar]: foo.md#test")
                .Assert(WarningIDs.UnusedDefinedReference, 0, 17, "Bar")
                .RemoveEntity("fOo.mD")
                .AssertContains(WarningIDs.UnresolvedReference, 0, 17, "foo.md#test")
                .Clear()
                .AssertNoWarnings()
                .Update("\n[]()\n\n# Foo \n")
                .Assert(
                    // Need a change in Markdig to get accurate LinkInline source spans for Label and Url
                    (WarningIDs.EmptyLinkContent, 1, 4, string.Empty),
                    (WarningIDs.EmptyReference, 1, 4, string.Empty),
                    (WarningIDs.HeadingEndsWithWhitespace, 7, 12, "# Foo "))
                .Clear()
                .Update("[Test]", "Bar.md")
                .Assert(WarningIDs.UnresolvedReference, 0, 5, "Test", "Bar.md")
                .AssertContains(w => w.IsError)
                .AssertSingle(w => w.Message.Length > 5);
        }
    }
}
