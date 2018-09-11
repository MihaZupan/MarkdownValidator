/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
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
        
        public static bool IsAny(this string source, StringComparison stringComparison, params string[] values)
        {
            foreach (string value in values)
            {
                if (source.Equals(value, stringComparison))
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

        public static bool ContainsAny(this string source, int startIndex, int endIndex, params char[] values)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                for (int j = 0; j < values.Length; j++)
                {
                    if (source[i] == values[j])
                        return true;
                }
            }
            return false;
        }
    }

    internal static class SourceHelpers
    {
        public static SourceSpan ToSpan(this StringSlice stringSlice)
            => new SourceSpan(stringSlice.Start, stringSlice.End);

        public static StringSlice Reposition(this StringSlice slice, SourceSpan span)
            => new StringSlice(slice.Text, span.Start, span.End);

        public static string Substring(this StringSlice source, SourceSpan span, int cutFromEnd = 0)
        {
            return source.Text.Substring(span.Start, span.Length - cutFromEnd);
        }

        public static bool Contains(this StringSlice slice, string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
            => slice.Text.IndexOf(value, slice.Start, slice.Length, stringComparison) != -1;
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
}
