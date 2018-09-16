/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Warnings;
using System.Linq;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    static class SingleFileTest
    {
        public const string DefaultFileName = "DefaultTestFile.md";

        public static void AssertWarnings(
            string source,
            params (WarningID ID, int Line, int Start, int End, string Value)[] warnings)
        {
            var report = ValidationReportProvider.GetReport(DefaultFileName, source);
            WarningComparer.AssertMatch(report.Warnings, warnings.ToList(), DefaultFileName);
        }

        public static void AssertNoWarnings(string source, bool fully = true)
        {
            var report = ValidationReportProvider.GetReport(DefaultFileName, source, fully);
            Assert.Empty(report.Warnings);
        }

        public static void AssertGlobalWarning(string source, WarningID id, string value = "")
            => AssertWarnings(source, (id, 0, SourceSpan.Empty.Start, SourceSpan.Empty.End, value));

        public static void AssertContainsWarning(string source, WarningID id, bool fully = true)
        {
            var report = ValidationReportProvider.GetReport(DefaultFileName, source, fully);
            WarningComparer.AssertContains(report.Warnings, id);
        }

        public static void AssertNotPresent(string source, WarningID id, bool fully = true)
        {
            var report = ValidationReportProvider.GetReport(DefaultFileName, source, fully);
            WarningComparer.AssertNotContains(report.Warnings, id);
        }

        public static void AssertNotPresent(string source, params WarningID[] ids)
        {
            var report = ValidationReportProvider.GetReport(DefaultFileName, source);
            foreach (var id in ids)
                WarningComparer.AssertNotContains(report.Warnings, id);
        }
    }
}
