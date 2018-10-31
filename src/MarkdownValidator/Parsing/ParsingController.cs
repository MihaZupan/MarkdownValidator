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
        private readonly Dictionary<Type, List<(string ParserIdentifier, Action<ParsingContext> ParsingAction)>> Parsers =
            new Dictionary<Type, List<(string ParserIdentifier, Action<ParsingContext> ParsingAction)>>();

        private readonly List<(string ParserIdentifier, Action<ParsingContext> Finalizer)> RegisteredFinalizers =
            new List<(string ParserIdentifier, Action<ParsingContext> Finalizer)>();

        internal readonly Config Configuration;

        public ParsingController(Config configuration)
        {
            Configuration = configuration;
            var registration = new ParserRegistrationContext(this, configuration);

            var internalParsers = new IParser[]
            {
                new LiteralInlineParser(),
                new LinkInlineParser(),
                new AutoLinkInlineParser(),
                new LinkReferenceDefinitionParser(),
                new HeadingBlockParser(),
                new FootnoteParser(),
                new CodeBlockParser(),
            };

            foreach (var parser in internalParsers)
            {
                registration.ParserIdentifier = parser.Identifier;
                parser.Initialize(registration);
            }
            foreach (var parser in Configuration.Parsing.Parsers)
            {
                registration.ParserIdentifier = parser.Identifier;
                parser.Initialize(registration);
            }
        }

        public void Register(Type type, string identifier, Action<ParsingContext> action)
        {
            var entry = (identifier, action);
            if (Parsers.TryGetValue(type, out var actions))
            {
                actions.Add(entry);
            }
            else
            {
                Parsers.Add(type, new List<(string, Action<ParsingContext>)>(1) { entry });
            }
        }
        public void RegisterFinalizer(string identifier, Action<ParsingContext> action)
            => RegisteredFinalizers.Add((identifier, action));

        public void Parse(MarkdownObject objectToParse, MarkdownFile sourceFile)
        {
            if (Parsers.TryGetValue(objectToParse.GetType(), out var parsers))
            {
                ParsingContext context = sourceFile.ParsingContext;
                context.Update(objectToParse);
                foreach (var (identifier, action) in parsers)
                {
                    context.SetWarningSource(WarningSource.ExternalParser, identifier);
                    action(context);
                }
            }
        }
        public void Finalize(MarkdownFile sourceFile)
        {
            ParsingContext context = sourceFile.ParsingContext;
            context.Update(null);
            foreach (var (identifier, finalizer) in RegisteredFinalizers)
            {
                context.SetWarningSource(WarningSource.ExternalParserFinalize, identifier);
                finalizer(context);
            }
        }
    }
}
