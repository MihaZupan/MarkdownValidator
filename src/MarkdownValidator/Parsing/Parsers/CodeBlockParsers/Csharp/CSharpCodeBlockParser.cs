/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers.CodeBlockParsers.Csharp
{
    public abstract class CSharpCodeBlockParser : ICodeBlockParser
    {
        public bool SupportsUndefinedLanguages => false;
        public bool SupportsLanguage(string info)
            => info.IsAny(StringComparison.OrdinalIgnoreCase, "csharp", "c#", "cs");

        public abstract void ParseCodeBlock(StringSlice codeBlock, ParsingContext context);
    }
}
