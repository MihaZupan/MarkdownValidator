/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    static class SourceHelper
    {
        public static string CleanSource(string source)
        {
            if (source.IndexOf("\r\n", StringComparison.Ordinal) != -1)
            {
                return source.Replace("\r\n", "\n", StringComparison.Ordinal);
            }
            else return source;
        }
    }
}
