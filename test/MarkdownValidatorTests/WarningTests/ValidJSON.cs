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
    public class ValidJSON
    {
        [Fact]
        public void Valid_Empty()
        {
            string source = @"
```json
{}
```
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void Valid()
        {
            string source = @"
```json
{
    ""test"": 123
}
```
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void Invalid()
        {
            string source = @"
```json
1-3
```
";
            SingleFileTest.AssertWarning(source,
                ExternalWarningIDs.InvalidJsonInJsonCodeBlock, 9, 11, "1-3");
        }
    }
}
