/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Tests.Framework;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.ContextTests
{
    public class DirectoryWithTrailingSlash
    {
        [Fact]
        public void IsRecognised()
        {
            var test = new RollingContextTest()
                .AddEntity("test-dir")
                .AddEntity("slash-dir/")
                .AddEntity("backslash-dir\\")
                .Update(@"
[bare](test-dir)
[slash](test-dir/)
")
                .AssertNoWarnings()
                .Update(@"
[bare](slash-dir)
[slash](slash-dir/)
")
                .AssertNoWarnings()
                .Update(@"
[bare](backslash-dir)
[slash](backslash-dir/)
")
                .AssertNoWarnings()
                .Update("[foo](test-dir\\)")
                .AssertHasWarnings();
        }
    }
}