/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Parsing;
using System;
using System.Diagnostics;

namespace MihaZupan.MarkdownValidator.Warnings
{
    [DebuggerDisplay("{Line}, {Span.Start}-{Span.End} {RelativeFilePath}")]
    public sealed class WarningLocation : IEquatable<WarningLocation>, IComparable<WarningLocation>
    {
        public readonly string FullFilePath;
        public readonly string RelativeFilePath;
        /// <summary>
        /// Zero-based line index of the warning location in the source file
        /// </summary>
        public readonly int Line = -1;
        public readonly SourceSpan Span = SourceSpan.Empty;
        public readonly bool RefersToEntireFile = true;

        internal WarningLocation(MarkdownFile file, Reference reference)
            : this(file.FullPath, file.RelativePath, reference.Line, reference.SourceSpan)
        { }
        /// <summary>
        /// The warning applies to a specific section in a file
        /// </summary>
        internal WarningLocation(MarkdownFile file, int line, SourceSpan span)
            : this(file.FullPath, file.RelativePath, line, span)
        { }
        /// <summary>
        /// The warning applies to the entire file
        /// </summary>
        internal WarningLocation(MarkdownFile file)
            : this(file.FullPath, file.RelativePath)
        { }
        /// <summary>
        /// The warning applies to a specific section in a file
        /// </summary>
        public WarningLocation(string fullPath, string relativePath, int line, SourceSpan span)
            : this(fullPath, relativePath)
        {
            RefersToEntireFile = line < 0;

            Line = line;
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
                hash = (hash * 397) ^ Line;
                hash = (hash * 397) ^ FullFilePath.Length;
                return hash;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is SourceSpan other)
            {
                return Equals(other);
            }
            return false;
        }
        public bool Equals(WarningLocation other)
        {
            if (other is null) return false;
            return other.Line == Line &&
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
                compare = RelativeFilePath.CompareTo(other.RelativeFilePath);

                if (compare == 0)
                {
                    // Sort by line number
                    compare = Line.CompareTo(other.Line);

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
            return compare;
        }
    }
}
