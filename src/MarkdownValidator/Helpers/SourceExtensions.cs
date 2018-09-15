/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System;

namespace MihaZupan.MarkdownValidator
{
    internal static class SourceExtensions
    {
        public static SourceSpan ToSpan(this StringSlice stringSlice)
            => new SourceSpan(stringSlice.Start, stringSlice.End);

        public static StringSlice Reposition(this StringSlice slice, SourceSpan span)
            => new StringSlice(slice.Text, span.Start, span.End);

        public static string Substring(this StringSlice source, SourceSpan span, int cutFromEnd = 0)
            => source.Text.Substring(span.Start, span.Length - cutFromEnd);

        public static string Substring(this StringSlice source, int startIndex, int length)
            => source.Text.Substring(startIndex, length);

        public static bool Contains(this StringSlice slice, string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
            => slice.Text.IndexOf(value, slice.Start, slice.Length, stringComparison) != -1;

        public static bool Contains(this LiteralInline literal, char value)
            => literal.Content.Text.IndexOf(value, literal.Span.Start, literal.Span.Length) != -1;

        public static bool Contains(this LiteralInline literal, char value, out int index)
        {
            index = literal.Content.Text.IndexOf(value, literal.Content.Start, literal.Content.Length);
            if (index == -1)
                return false;

            index -= literal.Content.Start;
            return true;
        }
    }
}
