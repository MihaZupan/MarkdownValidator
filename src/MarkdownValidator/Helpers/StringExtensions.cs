﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.Helpers
{
    public static class StringExtensions
    {
        public static bool OrdinalEquals(this string source, string value)
            => source.Equals(value, StringComparison.Ordinal);

        public static bool OrdinalContains(this string source, char value)
            => Contains(source, value, out _);

        public static bool Contains(this string source, char value, out int index, StringComparison comparison = StringComparison.Ordinal)
        {
            index = source.IndexOf(value, comparison);
            return index != -1;
        }

        public static bool OrdinalContainsAny(this string source, params char[] values)
        {
            foreach (var value in values)
            {
                if (source.OrdinalContains(value))
                    return true;
            }
            return false;
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

        public static bool IsEffectivelyEmpty(this string source)
        {
            if (source.Length == 0) return true;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (c != ' ' && c != '\n' && c != '\r')
                    return false;
            }
            return true;
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

        public static bool OrdinalContains(this string source, string value, int startIndex, int count)
            => source.IndexOf(value, startIndex, count, StringComparison.Ordinal) != -1;

        public static bool Contains(this string source, char value, int offset = 0)
            => source.IndexOf(value, offset) != -1;

        public static bool OrdinalStartsWith(this string source, string value)
            => source.StartsWith(value, StringComparison.Ordinal);

        public static int OrdinalIndexOf(this string source, string value, int startIndex = 0)
            => source.IndexOf(value, startIndex, StringComparison.Ordinal);
    }
}
