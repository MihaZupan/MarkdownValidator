/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing;
using System;

namespace MihaZupan.MarkdownValidator.ExternalParsers.CodeBlockParsers.JSON
{
    public abstract class JsonCodeBlockParser : ICodeBlockParser
    {
        public abstract string Identifier { get; }

        public bool SupportsUndefinedLanguages => false;

        public bool SupportsLanguage(string info)
            => info.Equals("json", StringComparison.OrdinalIgnoreCase);

        public virtual void Initialize(Config configuration) { }

        public abstract void ParseCodeBlock(CodeBlockInfo codeBlock, ParsingContext context);
    }
}
