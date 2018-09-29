/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    static class WarningComparer
    {
        public static void AssertMatch(
            Warning actual,
            (WarningID ID, int Start, int End, string Value) expected,
            string fileName)
            => AssertMatch(actual, fileName, expected.ID, expected.Start, expected.End, expected.Value);

        public static void AssertMatch(
           Warning actual,
           string fileName, WarningID id, int start, int end, string value)
        {
            Assert.Equal(fileName, actual.Location.RelativeFilePath);
            Assert.Equal(id, actual.ID);
            Assert.Equal(start, actual.Location.Span.Start);
            Assert.Equal(end, actual.Location.Span.End);
            Assert.Equal(value, actual.Value);
        }

        public static void AssertMatch(
            List<Warning> reportWarnings,
            List<(WarningID ID, int Start, int End, string Value)> expected, string fileName)
        {
            Assert.Equal(expected.Count, reportWarnings.Count);

            reportWarnings.Sort();

            var sortedExpected = expected
                .Select(w => (new WarningLocation(fileName, fileName, new SourceSpan(w.Start, w.End)), w.ID, w.Value))
                .OrderBy(w => w.Item1)
                .ThenBy(w => w.ID)
                .ThenBy(w => w.Value);

            int i = 0;
            foreach (var expectedWarning in sortedExpected)
            {
                var actualWarning = reportWarnings[i++];
                AssertMatch(
                    actualWarning,
                    expectedWarning.Item1.RelativeFilePath,
                    expectedWarning.ID,
                    expectedWarning.Item1.Span.Start,
                    expectedWarning.Item1.Span.End,
                    expectedWarning.Value);
            }
        }

        public static void AssertMatch(
            Dictionary<string, List<Warning>> reportWarningsByFile,
            params (string FileName, WarningID ID, int Start, int End, string Value)[] expected)
        {
            Assert.Equal(expected.Length, reportWarningsByFile.Sum(f => f.Value.Count));

            var expectedWarningsByFileName = expected
                .GroupBy(w => w.FileName)
                .Select(g => g.ToList())
                .ToList();

            Assert.Equal(expectedWarningsByFileName.Count, reportWarningsByFile.Count);

            foreach (var expectedWarnings in expectedWarningsByFileName)
            {
                string fileName = expectedWarnings.First().FileName;

                Assert.True(reportWarningsByFile.TryGetValue(fileName, out List<Warning> warnings));

                AssertMatch(
                    warnings,
                    expectedWarnings.Select(w => (w.ID, w.Start, w.End, w.Value)).ToList(),
                    expectedWarnings.First().FileName);
            }
        }

        public static void AssertContains(Dictionary<string, List<Warning>> reportWarnings, WarningID id)
        {
            bool contains = Contains(reportWarnings, id);
            Assert.True(contains, $"The report does not contain a(n) {id} warning");
        }

        public static void AssertNotContains(Dictionary<string, List<Warning>> reportWarnings, WarningID id)
        {
            bool contains = Contains(reportWarnings, id);
            Assert.False(contains, $"The report does contain a(n) {id.Identifier} warning");
        }
        private static bool Contains(Dictionary<string, List<Warning>> reportWarnings, WarningID id)
            => reportWarnings.ContainsAny(f => f.Value.ContainsAny(w => w.ID == id));
    }
}
