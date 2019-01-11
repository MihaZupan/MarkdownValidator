/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MihaZupan.MarkdownValidator.Warnings
{
    [DebuggerDisplay("{StartLine}-{EndLine}, {Span.Start}-{Span.End} {RelativeFilePath}")]
    public sealed class WarningLocation : IEquatable<WarningLocation>, IComparable<WarningLocation>
    {
        public readonly string FullFilePath;
        public readonly string RelativeFilePath;
        public readonly SourceSpan Span = SourceSpan.Empty;
        public readonly bool RefersToEntireFile = true;
        // Keeps us from having to have a Markdig reference on some projects
        public int Length => Span.Length;
        public bool IsSingleLine => StartLine == EndLine;

        /// <summary>
        /// Will be -1 if refering to the entire file. Zero-based
        /// </summary>
        public readonly int StartLine = -1;
        /// <summary>
        /// Will be -1 if refering to the entire file. Zero-based
        /// </summary>
        public readonly int EndLine = -1;
        /// <summary>
        /// Will be -1 if refering to the entire file. Zero-based
        /// </summary>
        public readonly int StartLineColumn = -1;
        /// <summary>
        /// Will be -1 if refering to the entire file. Zero-based
        /// </summary>
        public readonly int EndLineColumn = -1;

        internal WarningLocation(MarkdownFile file, Reference reference)
            : this(file, reference.SourceSpan)
        { }
        /// <summary>
        /// The warning applies to a specific section in a file
        /// </summary>
        internal WarningLocation(MarkdownFile file, SourceSpan span)
            : this(file.FullPath, file.RelativePath, span)
        {
            if (!RefersToEntireFile)
            {
                List<int> lineIndexes = file.ParsingResult.LineStartIndexes;

                int index = lineIndexes.BinarySearch(span.Start);
                if (index < 0) index = ~index - 1;

                StartLine = index;
                StartLineColumn = span.Start - lineIndexes[index];

                if (++index == lineIndexes.Count || lineIndexes[index] > span.End)
                {
                    EndLine = index - 1;
                    EndLineColumn = StartLineColumn + span.Length;
                    return;
                }

                index = lineIndexes.BinarySearch(index, lineIndexes.Count - index, span.End, Comparer<int>.Default);
                if (index < 0) index = ~index - 1;

                EndLine = index;
                EndLineColumn = span.End - lineIndexes[index];
            }
        }
        /// <summary>
        /// The warning applies to the entire file
        /// </summary>
        internal WarningLocation(MarkdownFile file)
            : this(file.FullPath, file.RelativePath)
        { }
        /// <summary>
        /// The warning applies to a specific section in a file
        /// </summary>
        public WarningLocation(string fullPath, string relativePath, SourceSpan span)
            : this(fullPath, relativePath)
        {
            RefersToEntireFile = Span.Equals(span);
            Span = span;
        }
        /// <summary>
        /// The warning applies to the entire file
        /// </summary>
        public WarningLocation(string fullPath, string relativePath)
        {
            FullFilePath = fullPath;
            RelativeFilePath = relativePath;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Span.GetHashCode();
                hash = (hash * 397) ^ StartLine;
                hash = (hash * 397) ^ EndLineColumn;
                hash = (hash * 397) ^ FullFilePath.Length;
                return hash;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is WarningLocation other)
            {
                return Equals(other);
            }
            return false;
        }
        public bool Equals(WarningLocation other)
        {
            if (other is null) return false;
            return 
                other.StartLine == StartLine &&
                other.StartLineColumn == StartLineColumn &&
                other.EndLine == EndLine &&
                other.EndLineColumn == EndLineColumn &&
                other.Span.Equals(Span) &&
                other.FullFilePath.OrdinalEquals(FullFilePath) &&
                other.RelativeFilePath.Length == RelativeFilePath.Length;
        }
        public static bool operator ==(WarningLocation a, WarningLocation b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(WarningLocation a, WarningLocation b)
            => !(a == b);

        public int CompareTo(WarningLocation other)
        {
            // Separate warnings that refer to the entire file
            int compare = RefersToEntireFile.CompareTo(other.RefersToEntireFile);

            if (compare == 0)
            {
                // Sort by name
                compare = StringComparer.OrdinalIgnoreCase.Compare(RelativeFilePath, other.RelativeFilePath);

                if (compare == 0)
                {
                    // Sort by start line number
                    compare = StartLine.CompareTo(other.StartLine);

                    if (compare == 0)
                    {
                        // Sort by start line column
                        compare = StartLineColumn.CompareTo(other.StartLineColumn);

                        if (compare == 0)
                        {
                            // Sort by end line number
                            compare = EndLine.CompareTo(other.EndLine);

                            if (compare == 0)
                            {
                                // Sort by end line column
                                compare = EndLineColumn.CompareTo(other.EndLineColumn);

                                if (compare == 0)
                                {
                                    // Sort by position in the file
                                    compare = Span.Start.CompareTo(other.Span.Start);

                                    if (compare == 0)
                                    {
                                        // Sort by length
                                        compare = Span.Length.CompareTo(other.Span.Length);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return compare;
        }
    }
}
