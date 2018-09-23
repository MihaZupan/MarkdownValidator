/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public class WebIOConfig
    {
        /// <summary>
        /// Defaults to 16
        /// </summary>
        public int MaximumRequestConcurrency
        {
            get => _maximumRequestConcurrency;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaximumRequestConcurrency), value, "Must be a positive integer");
                _maximumRequestConcurrency = Math.Min(1024, value);
            }
        }
        private int _maximumRequestConcurrency = 16;

        /// <summary>
        /// Setting this to a non-positive value will make the parser stop trying after the first redirect
        /// <para>Defaults to 4</para>
        /// </summary>
        public int MaximumRedirectCount = 4;
    }
}
