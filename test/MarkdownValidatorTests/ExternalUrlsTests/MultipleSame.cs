/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Tests.Framework;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.ExternalUrlsTests
{
    [Trait("Category", "WebIO")]
    public class MultipleSame
    {
        [Fact]
        public void MultipleSameHostname()
        {
            string source = @"
[Stuff] [Things]

[Foo](https://www.youtube.com/watch?v=gkTb9GP9lVI)
[Bar](https://www.youtube.com/watch?v=YtbJ8ZsiktI)

[Stuff]: https://www.youtube.com/watch?v=68ugkg9RePc
[Things]: https://www.youtube.com/watch?v=dQw4w9WgXcQ
";
            SingleFileTest.AssertNoWarnings(source);
        }

        [Fact]
        public void MultipleSameUrl()
        {
            string source = @"
[Stuff] [Things]

[Foo](https://www.google.com/search?q=Hello+world)
[Bar](https://www.google.com/search?q=Hello+world)

[Stuff]: https://www.google.com/search?q=Hello+world#123
[Things]: https://www.google.com/search?q=Hello+world
";
            SingleFileTest.AssertNoWarnings(source);
        }
    }
}
