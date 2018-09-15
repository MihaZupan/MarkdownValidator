/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Warnings;
using System.Collections.Generic;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    static class WarningComparer
    {
        public static void AssertMatch(
            List<Warning> reportWarnings,
            IList<(WarningID ID, int Line, int Start, int End, string Value)> expected)
        {
            Assert.Equal(expected.Count, reportWarnings.Count);
            for (int i = 0; i < reportWarnings.Count; i++)
            {
                var (ID, Line, Start, End, Value) = expected[i];
                var actual = reportWarnings[i];

                Assert.Equal(ID, actual.ID);
                Assert.Equal(Line, actual.Location.Line + 1);
                Assert.Equal(Start, actual.Location.Span.Start);
                Assert.Equal(End, actual.Location.Span.End);
                Assert.Equal(Value, actual.Value);
            }
        }

        public static void AssertContains(List<Warning> reportWarnings, WarningID id)
        {
            bool contains = false;
            foreach (var warning in reportWarnings)
            {
                if (warning.ID == id)
                {
                    contains = true;
                    break;
                }
            }
            Assert.True(contains, $"The report does not contain a(n) {id} warning");
        }

        public static void AssertNotContains(List<Warning> reportWarnings, WarningID id)
        {
            bool contains = false;
            foreach (var warning in reportWarnings)
            {
                if (warning.ID == id)
                {
                    contains = true;
                    break;
                }
            }
            Assert.False(contains, $"The report does contain a(n) {id} warning");
        }
    }
}
