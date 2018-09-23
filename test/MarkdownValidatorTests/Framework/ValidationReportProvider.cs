/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    static class ValidationReportProvider
    {
        private static readonly string RootDirectory = Path.GetFullPath("TestDirectory");

        private static readonly MarkdownContextValidator Validator
            = new MarkdownContextValidator(new Config(RootDirectory));

        public static ValidationReport GetReport(IEnumerable<(string Name, string Source)> files, IEnumerable<string> entities, bool fully)
        {
            lock (Validator)
            {
                Validator.Clear();
                foreach (var (Name, Source) in files)
                {
                    Validator.AddMarkdownFile(Path.Combine(RootDirectory, Name), SourceHelper.CleanSource(Source));
                }
                foreach (var entity in entities)
                {
                    Validator.AddEntity(Path.Combine(RootDirectory, entity));
                }

                var report = Validator.Validate(fully);

                if (fully)
                    Assert.True(report.IsComplete);

                return report;
            }
        }

        public static ValidationReport GetReport(IEnumerable<(string Name, string Source)> files, params string[] entities)
            => GetReport(files, entities, true);
        public static ValidationReport GetReport(params (string Name, string Source)[] files)
            => GetReport(files, Array.Empty<string>(), true);
        public static ValidationReport GetReport(string name, string source, IEnumerable<string> entities, bool fully = true)
            => GetReport(new[] { (name, source) }, entities, fully);
        public static ValidationReport GetReport(string name, string source, params string[] entities)
            => GetReport(name, source, entities, true);
        public static ValidationReport GetReport(string name, string source, bool fully)
            => GetReport(name, source, Array.Empty<string>(), fully);
    }
}
