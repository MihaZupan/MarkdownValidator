/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    static class SingleFileTest
    {
        public static void AssertNoWarnings(string source)
            => AssertWarnings(source);

        public static void AssertWarnings(
            string source,
            params (WarningID ID, int Line, int Start, int End, string Value)[] warnings)
        {
            Config configuration = new Config("TestDirectory");

            MarkdownValidator validator = new MarkdownValidator(configuration);
            validator.AddMarkdownFile("TestDirectory/Test0.md", source);
            var report = validator.ValidateFully();

            Assert.True(report.IsComplete);

            Assert.Equal(warnings.Length, report.Warnings.Count);
            for (int i = 0; i < warnings.Length; i++)
            {
                var (ID, Line, Start, End, Value) = warnings[i];
                var actual = report.Warnings[i];

                Assert.Equal(ID,        actual.ID);
                Assert.Equal(Line - 1,  actual.Location.Line);
                Assert.Equal(Start,     actual.Location.Span.Start);
                Assert.Equal(End,       actual.Location.Span.End);
                Assert.Equal(Value,     actual.Value);
            }
        }
    }
}
