/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Helpers
{
    public static class LinqHelper
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
        public static int FindIndex<T>(this IEnumerable<T> enumerable, T value, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;

            int count = 0;
            foreach (var element in enumerable)
            {
                if (comparer.Equals(element, value))
                    return count;
                count++;
            }
            return -1;
        }
        public static bool All<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            foreach (var element in enumerable)
            {
                if (!predicate(element))
                    return false;
            }
            return true;
        }
        public static bool All<ElementType>(this IEnumerable<object> enumerable, Predicate<ElementType> predicate)
        {
            foreach (var element in enumerable)
            {
                if (!(element is ElementType castElement))
                    return false;

                if (!predicate(castElement))
                    return false;
            }
            return true;
        }
    }
}
