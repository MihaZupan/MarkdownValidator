/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator
{
    public class ValidationReport : IEquatable<ValidationReport>
    {
        /// <summary>
        /// Indicates whether async operations (such as network IO) are still happening internally in the context
        /// <para>The system that requested validation should retry after a short period of time to get the updated report</para>
        /// <para>To always get the entire report, use <see cref="MarkdownContextValidator.ValidateFully"/> instead</para>
        /// </summary>
        public readonly bool IsComplete;

        /// <summary>
        /// Indicates how many milliseconds to wait before calling <see cref="MarkdownContextValidator.Validate"/> again (in case <see cref="IsComplete"/> is false
        /// <para>If <see cref="IsComplete"/> is set to true, <see cref="SuggestedWait"/> will have a value of -1</para>
        /// </summary>
        public readonly int SuggestedWait;

        public readonly Config Configuration;

        /// <summary>
        /// All warnings collected by the <see cref="MarkdownContextValidator"/>
        /// </summary>
        public readonly Dictionary<string, List<Warning>> WarningsByFile = new Dictionary<string, List<Warning>>(StringComparer.Ordinal);
        public int WarningCount { get; private set; } = 0;

        internal ValidationReport(Config configuration, bool complete, int suggestedWait = -1)
        {
            Configuration = configuration;
            IsComplete = complete;
            SuggestedWait = IsComplete ? -1 : suggestedWait;
        }

        internal ValidationReport AddWarning(WarningID id, WarningLocation location, string value, string messageFormat, params string[] messageArgs)
            => AddWarning(id, location, value, WarningSource.Validator, messageFormat, messageArgs);
        internal ValidationReport AddWarning(WarningID id, WarningLocation location, string value, WarningSource source, string messageFormat, params string[] messageArgs)
        {
            AddWarning(
                new Warning(
                    id,
                    location,
                    value,
                    string.Format(messageFormat, messageArgs),
                    source,
                    nameof(ValidationContext)));
            return this;
        }

        internal void AddWarning(Warning warning)
        {
            WarningCount++;
            if (WarningsByFile.TryGetValue(warning.Location.RelativeFilePath, out List<Warning> warnings))
            {
                warnings.Add(warning);
            }
            else
            {
                WarningsByFile.Add(warning.Location.RelativeFilePath, new List<Warning>() { warning });
            }
        }
        internal void AddWarnings(List<Warning> warnings)
        {
            foreach (var warning in warnings)
                AddWarning(warning);
        }

        public static readonly ValidationReport Empty = new ValidationReport(new Config(string.Empty), true);

        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is ValidationReport other)
                return Equals(other);
            return false;
        }
        public bool Equals(ValidationReport other)
        {
            if (other is null)
                return false;
            return Diff(other).NoChange;
        }
        public static bool operator ==(ValidationReport a, ValidationReport b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(ValidationReport a, ValidationReport b)
            => !(a == b);

        public ReportDiff Diff(ValidationReport previousReport)
        {
            Dictionary<string, (List<Warning> removed, List<Warning> added)> affectedFiles =
                new Dictionary<string, (List<Warning> removed, List<Warning> added)>(StringComparer.Ordinal);

            foreach (var fileWarnings in WarningsByFile)
            {
                if (previousReport.WarningsByFile.TryGetValue(fileWarnings.Key, out List<Warning> previousWarnings))
                {
                    var (removed, added) = DiffHelper.Diff(fileWarnings.Value, previousWarnings);
                    if (removed.Count != 0 || added.Count != 0)
                    {
                        affectedFiles.Add(fileWarnings.Key, (removed, added));
                    }
                }
                else
                {
                    affectedFiles.Add(fileWarnings.Key, (new List<Warning>(), fileWarnings.Value));
                }
            }
            foreach (var fileWarnings in previousReport.WarningsByFile)
            {
                if (!WarningsByFile.ContainsKey(fileWarnings.Key))
                {
                    affectedFiles.Add(fileWarnings.Key, (fileWarnings.Value, new List<Warning>()));
                }
            }

            return new ReportDiff(affectedFiles);
        }
        public class ReportDiff
        {
            public readonly Dictionary<string, (List<Warning> removed, List<Warning> added)> AffectedFiles;
            public readonly bool NoChange;

            internal ReportDiff(Dictionary<string, (List<Warning>, List<Warning>)> affectedFiles)
            {
                AffectedFiles = affectedFiles;
                NoChange = AffectedFiles.Count == 0;
            }
        }
    }
}
