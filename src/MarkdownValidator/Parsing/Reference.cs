/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using System;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class Reference : IEquatable<Reference>
    {
        public readonly string RawReference;
        public readonly string GlobalReference;
        public readonly SourceSpan SourceSpan;
        public readonly int Line;
        public readonly bool IsImage;

        public Reference(string reference, string globalReference, SourceSpan span, int line, bool isImage = false)
        {
            RawReference = reference;
            GlobalReference = globalReference;
            SourceSpan = span;
            Line = line;
            IsImage = isImage;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SourceSpan.GetHashCode() * 397) ^ Line;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is Reference other)
            {
                return Equals(other);
            }
            return false;
        }
        public bool Equals(Reference other)
        {
            if (other is null) return false;
            return other.Line == Line &&
                other.SourceSpan.Equals(SourceSpan) &&
                other.IsImage == IsImage &&
                other.GlobalReference.OrdinalEquals(GlobalReference) &&
                other.RawReference.OrdinalEquals(RawReference);
        }
    }
}
