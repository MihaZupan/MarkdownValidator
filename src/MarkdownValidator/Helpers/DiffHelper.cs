/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System.Collections.Generic;
using System.Linq;

namespace MihaZupan.MarkdownValidator
{
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
}
