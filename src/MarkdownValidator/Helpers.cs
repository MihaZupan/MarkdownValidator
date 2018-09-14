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
using System.Collections.Generic;
using System.Linq;

namespace MihaZupan.MarkdownValidator
{
    internal static class StringHelpers
    {
        public static bool OrdinalEquals(this string source, string value)
            => source.Equals(value, StringComparison.Ordinal);

        public static bool Contains(this string source, char value, out int index, StringComparison comparison = StringComparison.Ordinal)
        {
            index = source.IndexOf(value, comparison);
            return index != -1;
        }

        public static bool ContainsAny(this string source, StringComparison stringComparison, params string[] values)
        {
            foreach (var value in values)
            {
                if (source.Contains(value, stringComparison))
                    return true;
            }
            return false;
        }

        public static bool IsAny(this string source, StringComparison stringComparison, params string[] values)
        {
            foreach (string value in values)
            {
                if (source.Equals(value, stringComparison))
                    return true;
            }
            return false;
        }

        public static bool StartsWithAny(this string source, StringComparison stringComparison, params string[] values)
        {
            foreach (var value in values)
            {
                if (source.StartsWith(value, stringComparison))
                    return true;
            }
            return false;
        }

        public static bool OrdinalContains(this string source, string value)
            => source.Contains(value, StringComparison.Ordinal);

        public static bool OrdinalContains(this string source, char value)
            => source.Contains(value, StringComparison.Ordinal);

        public static bool OrdinalStartsWith(this string source, string value)
            => source.StartsWith(value, StringComparison.Ordinal);

        public static string SubstringAfter(this string source, char value)
        {
            int charIndex = source.IndexOf(value);
            return source.Substring(charIndex + 1);
        }
    }

    internal static class SourceHelpers
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

    internal static class DiffHelper
    {
        public static (List<T> removed, List<T> added) Diff<T>(List<T> previous, List<T> current)
        {
            List<T> removed = new List<T>();
            List<T> added = new List<T>();

            if (previous.Count < 20)
            {
                foreach (var element in current)
                {
                    int index = previous.IndexOf(element);
                    if (index != -1)
                    {
                        previous.RemoveAt(index);
                    }
                    else
                    {
                        added.Add(element);
                    }
                }
                foreach (var previousWarning in previous)
                {
                    removed.Add(previousWarning);
                }
            }
            else
            {
                HashSet<T> previousElements = previous.ToHashSet();
                foreach (var element in current)
                {
                    if (previousElements.Contains(element))
                    {
                        previousElements.Remove(element);
                    }
                    else
                    {
                        added.Add(element);
                    }
                }
                foreach (var previousWarning in previousElements)
                {
                    removed.Add(previousWarning);
                }
            }

            return (removed, added);
        }
        public static (List<T> removed, List<T> added) Diff<T>(ICollection<T> previous, ICollection<T> current)
        {
            List<T> removed = new List<T>();
            List<T> added = new List<T>();

            HashSet<T> previousElements = previous.ToHashSet();
            foreach (var element in current)
            {
                if (previousElements.Contains(element))
                {
                    previousElements.Remove(element);
                }
                else
                {
                    added.Add(element);
                }
            }

            return (removed, added);
        }
    }

    internal static class LinqHelper
    {
        public static bool ContainsAny<T>(this IEnumerable<T> enumerable, Predicate<T> condition)
        {
            foreach (var element in enumerable)
            {
                if (condition(element))
                    return true;
            }
            return false;
        }
        public static bool ContainsAny<T>(this IEnumerable<T> enumerable, Predicate<T> condition, out T first)
        {
            foreach (var element in enumerable)
            {
                if (condition(element))
                {
                    first = element;
                    return true;
                }
            }
            first = default;
            return false;
        }
    }

    internal static class HostnameHelper
    {
        public static bool IsDocumentationHostname(string hostname)
        {
            if (hostname.OrdinalContains("...")) // http://...
                return true;

            if (hostname.IsAny(StringComparison.OrdinalIgnoreCase,
                "example.com",  // RFC2606
                "example.org",  // RFC2606
                "example.net")) // RFC2606
                return true;

            if (hostname.EndsWith(".example", StringComparison.OrdinalIgnoreCase)) // RFC2606
                return true;

            if (hostname.StartsWithAny(StringComparison.Ordinal,
                "192.0.2.",     // RFC5737
                "198.51.100.",  // RFC5737
                "203.0.113."))  // RFC5737
                return true;

            if (hostname.StartsWith("2001:DB8::", StringComparison.OrdinalIgnoreCase)) // RFC3849
                return true;

            return false;
        }
    }
}
