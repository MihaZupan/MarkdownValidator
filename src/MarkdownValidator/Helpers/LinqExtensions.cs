/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator
{
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
