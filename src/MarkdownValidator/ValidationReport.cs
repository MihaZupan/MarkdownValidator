/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Warnings;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator
{
    public class ValidationReport
    {
        /// <summary>
        /// Indicates whether async operations (such as network IO) are still happening internally in the context
        /// <para>The system that requested validation should retry after a short period of time to get the updated report</para>
        /// <para>To always get the entire report, pass true to <see cref="ValidationContext.Validate(bool)"/></para>
        /// </summary>
        public readonly bool IsComplete;

        /// <summary>
        /// Indicates how many milliseconds to wait before calling validate again (in case <see cref="IsComplete"/> is false
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

        internal void AddWarning(WarningID id, WarningLocation location, string messageFormat, params string[] messageArgs)
        {
            Warnings.Add(
                new Warning(
                    id,
                    location,
                    string.Format(messageFormat, messageArgs),
                    WarningSource.Validator));
        }

        internal (List<Warning> removed, List<Warning> added) Diff(ValidationReport previousReport)
        {
            return DiffHelper.Diff(previousReport.Warnings, Warnings);
        }
    }
}
