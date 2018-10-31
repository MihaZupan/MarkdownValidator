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

namespace MihaZupan.MarkdownValidator.Tests.WarningTests
{
    public class EffectivelyEmptyCodeBlock
    {
        [Fact]
        public void EffectivelyEmpty()
        {
            string source = @"
```
 

```
";
            SingleFileTest.AssertWarning(source,
                WarningIDs.EffectivelyEmptyCodeBlock, 1, 10, string.Empty);
        }
    }
}
