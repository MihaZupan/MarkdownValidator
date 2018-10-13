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

namespace MihaZupan.MarkdownValidator.Tests.ExternalUrlsTests
{
    public class UrlRewriter
    {
        [Fact]
        public void Microsoft_ReferenceSource()
        {
            // If there was no rewriting done here, any url would be valid (note that 'Something' has been added below)
            string source = "[Foo](https://referencesource.microsoft.com/#SystemSomething/net/System/UriEnumTypes.cs,237c8dc4861d2cfa)";
            SingleFileTest.AssertContainsWarning(source,
                WarningIDs.WebRequestReturnedErrorCode);

            source = "[Foo](https://referencesource.microsoft.com/#System/net/System/UriEnumTypes.cs,237c8dc4861d2cfa)";
            SingleFileTest.AssertNoWarnings(source);
        }
    }
}
