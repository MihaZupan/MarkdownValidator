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
    public class OrderedList
    {
        [Fact]
        public void TestContext()
        {
            var test = new RollingContextTest()
                .Update(@"
1. Test
	1. Foo
	2. Bar
	2. Test
2. Something
3. Foo
5. Bar
3. Foo
")
                .Assert(
                    (WarningID.InvalidListNumberOrder, 5, 26, 32, "3-2"),
                    (WarningID.InvalidListNumberOrder, 8, 54, 59, "4-5"))
                .Update(@"
1. Test
	1. Foo
	2. Test
2. Something
3. Foo
4. Bar
18. Foo
")
                .Assert(WarningID.InvalidListNumberOrder, 8, 53, 59, "5-18")
                .Clear()
                .AssertNotPresent(WarningID.InvalidListNumberOrder)
                .Update("2. Foo")
                .Assert(WarningID.InvalidListNumberOrder, 1, 0, 5, "1-2")
                .Update("1. Bar")
                .AssertNoWarnings();
        }
    }
}
