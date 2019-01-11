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
                    (WarningIDs.InvalidListNumberOrder, 26, 32, "3-2"),
                    (WarningIDs.InvalidListNumberOrder, 54, 59, "4-5"))
                .Update(@"
1. Test
	1. Foo
	2. Test
2. Something
3. Foo
4. Bar
18. Foo
")
                .Assert(WarningIDs.InvalidListNumberOrder, 53, 59, "5-18")
                .Clear()
                .AssertNotPresent(WarningIDs.InvalidListNumberOrder)
                .Update("2. Foo")
                .Assert(WarningIDs.InvalidListNumberOrder, 0, 5, "1-2")
                .Update("1. Bar")
                .AssertNoWarnings();
        }
    }
}
