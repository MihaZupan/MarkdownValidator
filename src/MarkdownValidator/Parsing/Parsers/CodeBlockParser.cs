/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class CodeBlockParser : IParser
    {
        private bool ParseUndefinedLanguages;
        private Predicate<string> LanguageWhiteListTest;
        private Predicate<string> LanguageBlackListTest;
        private readonly Dictionary<string, List<ICodeBlockParser>> CommonLanguages
            = new Dictionary<string, List<ICodeBlockParser>>(StringComparer.OrdinalIgnoreCase);
        private readonly List<ICodeBlockParser> Parsers = new List<ICodeBlockParser>();
        private readonly List<ICodeBlockParser> UndefinedLanguageParsers = new List<ICodeBlockParser>();

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(FencedCodeBlock), ParseCodeBlock);

            var codeBlocksConfig = context.Configuration.Parsing.CodeBlocks;

            ParseUndefinedLanguages = codeBlocksConfig.ParseUndefinedLanguages;
            LanguageWhiteListTest = codeBlocksConfig.LanguageWhiteList;
            LanguageBlackListTest = codeBlocksConfig.LanguageBlackList;
            Parsers.AddRange(codeBlocksConfig.Parsers);

            if (ParseUndefinedLanguages)
            {
                foreach (var parser in Parsers)
                {
                    if (parser.SupportsUndefinedLanguages)
                    {
                        UndefinedLanguageParsers.Add(parser);
                    }
                }
            }

            foreach (var language in CodeBlocksConfig.CommonLanguages)
            {
                List<ICodeBlockParser> languageParsers = null;
                if (LanguageWhiteListTest(language) && !LanguageBlackListTest(language))
                {
                    languageParsers = new List<ICodeBlockParser>();
                    foreach (var parser in Parsers)
                    {
                        if (parser.SupportsLanguage(language))
                            languageParsers.Add(parser);
                    }
                    if (languageParsers.Count == 0)
                        languageParsers = null;
                }
                CommonLanguages.Add(language, languageParsers);
            }
        }

        private void ParseCodeBlock(ParsingContext context)
        {
            var codeBlock = context.Object as FencedCodeBlock;
            context.SetWarningSource(WarningSource.InternalCodeBlockParser);
            var originalObject = context.Object;

            if (codeBlock.Lines.Count == 0)
            {
                context.ReportWarning(WarningID.EmptyCodeBlock, string.Empty, "Code block is empty");
                return;
            }

            StringSlice slice = context.Source.Reposition(codeBlock.Span);
            List<ICodeBlockParser> parsers = null;

            if (codeBlock.Info is null)
            {
                if (ParseUndefinedLanguages)
                {
                    parsers = UndefinedLanguageParsers;
                }
            }
            else if (!CommonLanguages.TryGetValue(codeBlock.Info, out parsers))
            {
                if (LanguageWhiteListTest(codeBlock.Info) && !LanguageBlackListTest(codeBlock.Info))
                {
                    foreach (var parser in Parsers)
                    {
                        if (parser.SupportsLanguage(codeBlock.Info))
                        {
                            context.SetWarningSource(WarningSource.ExternalCodeBlockParser);
                            parser.ParseCodeBlock(slice, context);
                        }
                    }
                }
                return;
            }

            if (parsers is null)
                return;

            foreach (var parser in parsers)
            {
                context.SetWarningSource(WarningSource.ExternalCodeBlockParser);
                parser.ParseCodeBlock(slice, context);
            }
        }
    }
}
