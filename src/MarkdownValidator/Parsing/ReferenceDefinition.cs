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
    internal class ReferenceDefinition : Reference, IEquatable<ReferenceDefinition>
    {
        public readonly MarkdownFile SourceFile;
        public bool Used = false;

        public ReferenceDefinition(string reference, string globalReference, SourceSpan span, int line, MarkdownFile sourceFile)
            : base(reference, globalReference, span, line)
        {
            SourceFile = sourceFile;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is ReferenceDefinition definition)
            {
                return Equals(definition);
            }
            return false;
        }
        public bool Equals(ReferenceDefinition other)
        {
            if (!other.Equals((Reference)this)) return false;
            return SourceFile == other.SourceFile;
        }
    }
}
