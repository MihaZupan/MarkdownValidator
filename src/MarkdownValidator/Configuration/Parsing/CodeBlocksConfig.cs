/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public class CodeBlocksConfig
    {
        /// <summary>
        /// Defaults to true
        /// </summary>
        public bool ParseUndefinedLanguages = true;
        public Predicate<string> LanguageWhiteList = language => true;
        public Predicate<string> LanguageBlackList = language => false;

        internal List<ICodeBlockParser> Parsers = new List<ICodeBlockParser>();
        public void AddParser(ICodeBlockParser parser)
        {
            Parsers.Add(parser);
        }

        /// <summary>
        /// Parser lists for these languages will be pre-prepared to improve performance
        /// <para>Any language added here will be matched using <see cref="StringComparer.OrdinalIgnoreCase"/></para>
        /// </summary>
        /// <remarks>
        /// If you think there is a relevant language missing from this list, please open an issue/PR on GitHub
        /// </remarks>
        internal static readonly string[] CommonLanguages = new string[]
        {
            "text",
            "json", "diff", "csv", "xml", "yaml",

            "css",
            "sql",

            "shell", "sh", "bash",
            "console", "powershell",
            "vim", "viml",

            "csharp", "cs", "c#",
            "c",
            "cpp", "c++",
            "javascript", "js",
            "java",
            "go",
            "rb", "ruby",
            "rust",
            "swift",
            "objectivec",
            "perl",
            "php",
            "python", "py",
            "pascal",
            "matlab",
        };
    }
}
