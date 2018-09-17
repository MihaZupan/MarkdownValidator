/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing.Parsers;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal sealed class ParsingController
    {
        private readonly Dictionary<Type, List<Action<ParsingContext>>> Parsers =
            new Dictionary<Type, List<Action<ParsingContext>>>();

        private readonly List<Action<ParsingContext>> RegisteredFinalizers =
            new List<Action<ParsingContext>>();

        internal readonly Config Configuration;

        public ParsingController(Config configuration)
        {
            Configuration = configuration;
            var registration = new ParserRegistrationContext(this, configuration);

            // Internal parsers
            new MarkdownDocumentParser().Initialize(registration);
            new LiteralInlineParser().Initialize(registration);
            new LinkInlineParser().Initialize(registration);
            new LinkReferenceDefinitionParser().Initialize(registration);
            new HeadingBlockParser().Initialize(registration);
            new FootnoteParser().Initialize(registration);
            new CodeBlockParser().Initialize(registration);
            new ListBlockParser().Initialize(registration);

            foreach (var parser in Configuration.Parsing.Parsers)
            {
                parser.Initialize(registration);
            }
        }

        public void Register(Type type, Action<ParsingContext> action)
        {
            if (Parsers.TryGetValue(type, out List<Action<ParsingContext>> actions))
            {
                actions.Add(action);
            }
            else
            {
                Parsers.Add(type, new List<Action<ParsingContext>>() { action });
            }
        }
        public void RegisterFinalizer(Action<ParsingContext> action)
            => RegisteredFinalizers.Add(action);

        public void Parse(MarkdownObject objectToParse, MarkdownFile sourceFile)
        {
            if (Parsers.TryGetValue(objectToParse.GetType(), out List<Action<ParsingContext>> parsers))
            {
                ParsingContext context = sourceFile.ParsingContext;
                context.Update(objectToParse);
                foreach (var parserAction in parsers)
                {
                    context.SetWarningSource(WarningSource.ExternalParser);
                    parserAction(context);
                }
            }
        }
        public void Finalize(MarkdownFile sourceFile)
        {
            ParsingContext context = sourceFile.ParsingContext;
            context.Update(null);
            foreach (var finalizer in RegisteredFinalizers)
            {
                context.SetWarningSource(WarningSource.ExternalParserFinalize);
                finalizer(context);
            }
        }
    }
}
