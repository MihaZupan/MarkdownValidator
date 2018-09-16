/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Linq;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    class RollingContextTest
    {
        public const string DefaultFileName = "DefaultTestFile.md";
        private readonly MarkdownValidator Validator;
        public bool ValidateFully { get; private set; } = true;

        public RollingContextTest(string workingDirectory = "")
            : this(new Config(workingDirectory))
        { }
        public RollingContextTest(Config configuration)
        {
            configuration = configuration ?? new Config(string.Empty);
            Validator = new MarkdownValidator(configuration);
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

        public RollingContextTest AddEntity(string path)
        {
            Validator.AddEntity(path);
            return this;
        }

        public RollingContextTest RemoveEntity(string path)
        {
            Validator.RemoveEntity(path);
            return this;
        }

        public RollingContextTest Clear()
        {
            Validator.Clear();
            return this;
        }


        public RollingContextTest Assert(WarningID id, int line, int start, int end, string value, string fileName = DefaultFileName)
            => Assert((fileName, id, line, start, end, value));

        public RollingContextTest Assert(params (WarningID ID, int Line, int Start, int End, string Value)[] warnings)
            => Assert(warnings.Select(w => (DefaultFileName, w.ID, w.Line, w.Start, w.End, w.Value)).ToArray());

        public RollingContextTest Assert(params (string FileName, WarningID ID, int Line, int Start, int End, string Value)[] warnings)
        {
            var report = Validator.Validate(ValidateFully);
            WarningComparer.AssertMatch(report.Warnings, warnings);
            return this;
        }

        public RollingContextTest AssertContains(params WarningID[] ids)
        {
            var report = Validator.Validate(ValidateFully);
            foreach (var id in ids)
                WarningComparer.AssertContains(report.Warnings, id);
            return this;
        }

        public RollingContextTest AssertContains(WarningID id, int line, int start, int end, string value, string fileName = null)
        {
            var report = Validator.Validate(ValidateFully);
            Xunit.Assert.Contains(
                report.Warnings,
                w =>
                    w.ID == id &&
                    w.Location.Line == line - 1 &&
                    w.Location.Span == new SourceSpan(start, end) &&
                    w.Value == value &&
                    (fileName == null || w.Location.RelativeFilePath == fileName));
            return this;
        }

        public RollingContextTest AssertNotPresent(params WarningID[] ids)
        {
            var report = Validator.Validate(ValidateFully);
            foreach (var id in ids)
                WarningComparer.AssertNotContains(report.Warnings, id);
            return this;
        }

        public RollingContextTest AssertNoWarnings()
        {
            Xunit.Assert.Empty(Validator.Validate(ValidateFully).Warnings);
            return this;
        }

        public RollingContextTest AssertSingle(Predicate<Warning> warningPredicate)
        {
            var report = Validator.Validate(ValidateFully);
            Xunit.Assert.Single(report.Warnings);
            Xunit.Assert.True(warningPredicate(report.Warnings.Single()));
            return this;
        }

        public RollingContextTest AssertContains(Predicate<Warning> warningPredicate)
        {
            var report = Validator.Validate(ValidateFully);
            Xunit.Assert.Contains(report.Warnings, warningPredicate);
            return this;
        }
    }
}
