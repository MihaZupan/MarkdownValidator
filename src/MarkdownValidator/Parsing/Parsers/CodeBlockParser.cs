/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class CodeBlockParser : IParser
    {
        public string Identifier => nameof(CodeBlockParser);

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

            var configuration = context.Configuration;
            var codeBlocksConfig = configuration.Parsing.CodeBlocks;

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
                    languageParsers = new List<ICodeBlockParser>(4);
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

            foreach (var parser in Parsers)
            {
                parser.Initialize(configuration);
            }
        }

        private void ParseCodeBlock(ParsingContext context)
        {
            var codeBlock = context.Object as FencedCodeBlock;
            context.SetWarningSource(WarningSource.InternalCodeBlockParser);
            var originalObject = context.Object;

            if (codeBlock.Lines.Count == 0)
            {
                context.ReportWarning(WarningIDs.EmptyCodeBlock, string.Empty, "Code block is empty");
                return;
            }

            if (codeBlock.IsOpen)
            {
                context.ReportWarning(
                    WarningIDs.UnclosedCodeBlock,
                    codeBlock.Span.Start,
                    context.Source.Text.Length - 1,
                    string.Empty,
                    "Code block is never closed");
                return;
            }

            StringSlice slice = context.Source.Reposition(codeBlock.Span);
            var codeBlockInfo = new CodeBlockInfo(codeBlock, slice);

            if (codeBlockInfo.Content.IsEffectivelyEmpty())
            {
                context.ReportWarning(WarningIDs.EffectivelyEmptyCodeBlock, string.Empty, "Code block is effectively empty");
                return;
            }

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
                            context.SetWarningSource(WarningSource.ExternalCodeBlockParser, parser.Identifier);
                            parser.ParseCodeBlock(codeBlockInfo, context);
                        }
                    }
                }
                return;
            }

            if (parsers is null)
                return;

            foreach (var parser in parsers)
            {
                context.SetWarningSource(WarningSource.ExternalCodeBlockParser, parser.Identifier);
                parser.ParseCodeBlock(codeBlockInfo, context);
            }
        }
    }
}
