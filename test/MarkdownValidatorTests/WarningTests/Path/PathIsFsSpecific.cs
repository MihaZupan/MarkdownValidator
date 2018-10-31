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

namespace MihaZupan.MarkdownValidator.Tests.WarningTests.Path
{
    public class PathIsFsSpecific
    {
        [Fact]
        public void FsSpecific_Slash()
        {
            string source = @"[Foo](/bar)";
            SingleFileTest.AssertWarning(source,
                WarningIDs.PathIsFsSpecific, 6, 9, "/bar");
        }

        [Fact]
        public void FsSpecific_Tilde()
        {
            string source = @"[Foo](~bar)";
            SingleFileTest.AssertWarning(source,
                WarningIDs.PathIsFsSpecific, 6, 9, "~bar");
        }

        [Fact]
        public void FsSpecific_CColonSlash()
        {
            string source = @"[Foo](C:/bar)";
            SingleFileTest.AssertWarning(source,
                WarningIDs.PathIsFsSpecific, 6, 11, "C:/bar");
        }
    }
}
