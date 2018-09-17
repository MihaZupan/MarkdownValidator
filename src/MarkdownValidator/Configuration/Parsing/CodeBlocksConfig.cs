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
        /// <para>Any language added here will be matched with <see cref="StringComparer.OrdinalIgnoreCase"/></para>
        /// </summary>
        /// <remarks>
        /// If you think there is a relevant language missing from this list, please open an issue/PR on GitHub
        /// </remarks>
        public static readonly HashSet<string> CommonLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "text",
            "json", "diff", "csv", "xml", "yaml",

            "css",
            "http",
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

        /// <summary>
        /// This will be merged into <see cref="CommonLanguages"/> as soon as the <see cref="Config"/> instance is
        /// used in a <see cref="MarkdownValidator"/>
        /// <para>It is to be used exclusively for config importing before any parsing takes place</para>
        /// Defaults to an empty array
        /// </summary>
        public string[] MoreCommonLanguages = Array.Empty<string>();

        internal void Initialize()
        {
            foreach (var langauge in MoreCommonLanguages)
                CommonLanguages.Add(langauge);
        }
    }
}
