/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
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
        /// <para>To always get the entire report, use <see cref="MarkdownValidator.ValidateFully"/> instead</para>
        /// </summary>
        public readonly bool IsComplete;

        /// <summary>
        /// Indicates how many milliseconds to wait before calling <see cref="MarkdownValidator.Validate"/> again (in case <see cref="IsComplete"/> is false
        /// <para>If <see cref="IsComplete"/> is set to true, <see cref="SuggestedWait"/> will have a value of -1</para>
        /// </summary>
        public readonly int SuggestedWait;

        /// <summary>
        /// All warnings collected by the <see cref="MarkdownValidator"/>
        /// </summary>
        public readonly List<Warning> Warnings = new List<Warning>();

        internal ValidationReport(bool complete, int suggestedWait)
        {
            IsComplete = complete;
            SuggestedWait = IsComplete ? -1 : suggestedWait;
        }

        internal void AddWarning(WarningID id, WarningLocation location, string value, string messageFormat, params string[] messageArgs)
        {
            Warnings.Add(
                new Warning(
                    id,
                    location,
                    value,
                    string.Format(messageFormat, messageArgs),
                    WarningSource.Validator));
        }

        public (List<Warning> removed, List<Warning> added) Diff(ValidationReport previousReport)
        {
            return DiffHelper.Diff(previousReport.Warnings, Warnings);
        }

        public static readonly ValidationReport Empty = new ValidationReport(true, 0);

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

            if (IsComplete != other.IsComplete ||
                SuggestedWait != other.SuggestedWait ||
                Warnings.Count != other.Warnings.Count)
                return false;

            for (int i = 0; i < Warnings.Count; i++)
            {
                if (!Warnings[i].Equals(other.Warnings[i]))
                    return false;
            }
            return true;
        }
        public static bool operator ==(ValidationReport a, ValidationReport b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(ValidationReport a, ValidationReport b)
            => !(a == b);
    }
}
