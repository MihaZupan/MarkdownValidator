/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    class RollingContextTest
    {
        public const string DefaultFileName = "DefaultTestFile.md";
        private readonly MarkdownContextValidator Validator;
        public bool ValidateFully { get; private set; } = true;

        private ValidationReport Validate()
        {
            var report = Validator.Validate(ValidateFully);
            if (ValidateFully)
                Xunit.Assert.True(report.IsComplete);
            return report;
        }

        public RollingContextTest(string workingDirectory = "")
            : this(new Config(workingDirectory))
        { }
        public RollingContextTest(Config configuration)
        {
            configuration = configuration ?? new Config(string.Empty);
            Validator = new MarkdownContextValidator(configuration);
        }

        public RollingContextTest SetValidateFully(bool fully)
        {
            ValidateFully = fully;
            return this;
        }

        public RollingContextTest Update(string source, string fileName = DefaultFileName)
        {
            source = SourceHelper.CleanSource(source);
            if (!Validator.UpdateMarkdownFile(fileName, source))
                Validator.AddMarkdownFile(fileName, source);
            return this;
        }

        public RollingContextTest AddMarkdownFile(string source, string fileName = DefaultFileName, bool assertReturn = true)
        {
            source = SourceHelper.CleanSource(source);
            Xunit.Assert.Equal(assertReturn, Validator.AddMarkdownFile(fileName, source));
            return this;
        }

        public RollingContextTest AddEntity(string path, bool assertReturn = true)
        {
            Xunit.Assert.Equal(assertReturn, Validator.AddEntity(path));
            return this;
        }

        public RollingContextTest RemoveEntity(string path, bool assertReturn = true)
        {
            Xunit.Assert.Equal(assertReturn, Validator.RemoveEntity(path));
            return this;
        }

        public RollingContextTest Clear()
        {
            Validator.Clear();
            return this;
        }


        public RollingContextTest Assert(WarningID id, int start, int end, string value, string fileName = DefaultFileName)
            => Assert((fileName, id, start, end, value));

        public RollingContextTest Assert(params (WarningID ID, int Start, int End, string Value)[] warnings)
            => Assert(warnings.Select(w => (DefaultFileName, w.ID, w.Start, w.End, w.Value)).ToArray());

        public RollingContextTest Assert(params (string FileName, WarningID ID, int Start, int End, string Value)[] warnings)
        {
            var report = Validate();
            WarningComparer.AssertMatch(report.WarningsByFile, warnings);
            return this;
        }

        public RollingContextTest AssertContains(params WarningID[] ids)
        {
            var report = Validate();
            foreach (var id in ids)
                WarningComparer.AssertContains(report.WarningsByFile, id);
            return this;
        }

        public RollingContextTest AssertContains(WarningID id, int start, int end, string value, string fileName = null)
        {
            bool matches(Warning warning)
            {
                return warning.ID == id &&
                    warning.Location.Span == new SourceSpan(start, end) &&
                    warning.Value == value &&
                    (fileName == null || warning.Location.RelativeFilePath.OrdinalEquals(fileName));
            }

            var report = Validate();

            if (fileName != null)
            {
                Xunit.Assert.True(report.WarningsByFile.TryGetValue(fileName, out List<Warning> warnings));
                Xunit.Assert.Contains(warnings, w => matches(w));
            }
            else
            {
                Xunit.Assert.True(report.WarningsByFile.ContainsAny(f => f.Value.ContainsAny(w => matches(w))));
            }

            return this;
        }

        public RollingContextTest AssertNotPresent(params WarningID[] ids)
        {
            var report = Validate();
            foreach (var id in ids)
                WarningComparer.AssertNotContains(report.WarningsByFile, id);
            return this;
        }

        public RollingContextTest AssertNoWarnings()
        {
            var report = Validate();
            Xunit.Assert.Empty(report.WarningsByFile);
            return this;
        }

        public RollingContextTest AssertHasWarnings()
        {
            var report = Validate();
            Xunit.Assert.NotEmpty(report.WarningsByFile);
            return this;
        }

        public RollingContextTest AssertSingle(Predicate<Warning> warningPredicate)
        {
            var report = Validate();
            Xunit.Assert.Single(report.WarningsByFile);
            Xunit.Assert.Single(report.WarningsByFile.Single().Value);
            Xunit.Assert.True(warningPredicate(report.WarningsByFile.Single().Value.Single()));
            return this;
        }

        public RollingContextTest AssertContains(Predicate<Warning> warningPredicate)
        {
            var report = Validate();
            Xunit.Assert.True(report.WarningsByFile.ContainsAny(f => f.Value.ContainsAny(warningPredicate)));
            return this;
        }

        public RollingContextTest AssertWarningCount(int count)
        {
            var report = Validate();
            Xunit.Assert.Equal(count, report.WarningCount);
            return this;
        }
    }
}
