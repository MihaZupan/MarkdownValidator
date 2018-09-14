﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public interface ICodeBlockParser
    {
        bool SupportsUndefinedLanguages { get; }
        /// <summary>
        /// Returns true when the parser supports the language.
        /// <para>Must be case insensitive</para>
        /// </summary>
        /// <param name="info">The language info specified at the start of the code block</param>
        /// <returns></returns>
        bool SupportsLanguage(string info);
        void ParseCodeBlock(StringSlice codeBlock, ParsingContext context);
    }
}
