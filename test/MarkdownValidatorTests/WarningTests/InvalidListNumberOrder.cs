/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.ExternalParsers;
using MihaZupan.MarkdownValidator.Tests.Framework;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.WarningTests
{
    public class InvalidListNumberOrder
    {
        [Fact]
        public void NoWarning()
        {
            string source = @"
1. Foo
2. Bar
3. Test
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void NoWarning_AllOne()
        {
            string source = @"
1. Foo
1. Bar
1. Test
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void Invalid_Higher()
        {
            string source = @"
1. Foo
5. Bar
1. Test
";
            SingleFileTest.AssertWarning(source,
                ExternalWarningIDs.InvalidListNumberOrder, 8, 13, "2-5");
        }

        [Fact]
        public void Invalid_Lower()
        {
            string source = @"
1. Foo
2. Bar
1. Test
";
            SingleFileTest.AssertWarning(source,
                ExternalWarningIDs.InvalidListNumberOrder, 15, 21, "3-1");
        }
    }
}
