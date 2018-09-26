/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace MihaZupan.MarkdownValidator.Helpers
{
    public static class DiffHelper
    {
        public static (List<T> removed, List<T> added) Diff<T>(ICollection<T> previous, ICollection<T> current, IEqualityComparer<T> comparer = null)
            => Diff(previous, current, previous.Count, comparer);

        /// <param name="previousCount">Has to be AT LEAST the size of previous - can be more</param>
        public static (List<T> removed, List<T> added) Diff<T>(IEnumerable<T> previous, IEnumerable<T> current, int previousCount, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;

            List<T> removed = new List<T>();
            List<T> added = new List<T>();

            Span<bool> toDelete = stackalloc bool[previousCount];

            if (previousCount < 20)
            {
                foreach (var element in current)
                {
                    int index = previous.FindIndex(element, comparer);
                    if (index != -1)
                    {
                        toDelete[index] = true;
                    }
                    else
                    {
                        added.Add(element);
                    }
                }
            }
            else
            {
                int count = 0;
                Dictionary<T, int> previousElements = previous.ToDictionary(e => e, _ => count++, comparer);
                foreach (var element in current)
                {
                    if (previousElements.TryGetValue(element, out int index))
                    {
                        toDelete[index] = true;
                    }
                    else
                    {
                        added.Add(element);
                    }
                }
            }

            int deletedIndex = 0;
            foreach (var element in previous)
            {
                if (!toDelete[deletedIndex++])
                    removed.Add(element);
            }

            return (removed, added);
        }
    }
}
