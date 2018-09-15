/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Extensions.Footnotes;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class FootnoteParser : IParser
    {
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(FootnoteGroup), ParseFootnoteGroup);
            context.Register(typeof(FootnoteLinkReferenceDefinition), ParseFootnoteLinkReferenceDefinition);
            context.RegisterFinalizer(Finalize);
        }

        private void ParseFootnoteGroup(ParsingContext context)
        {
            var footnoteGroup = context.Object as FootnoteGroup;

            var (Defined, Accessed) = GetUsedOrders(context);
            foreach (Footnote footnote in footnoteGroup)
            {
                Accessed.Add(footnote.Order);
            }
        }

        private void ParseFootnoteLinkReferenceDefinition(ParsingContext context)
        {
            var linkDefinition = context.Object as FootnoteLinkReferenceDefinition;

            var (Defined, Accessed) = GetUsedOrders(context);
            Defined.Add((
                linkDefinition.Footnote.Order,
                new Reference(
                    linkDefinition.Label,
                    linkDefinition.Label,
                    linkDefinition.Span,
                    linkDefinition.Line)));
        }

        private void Finalize(ParsingContext context)
        {
            context.SetWarningSource(WarningSource.InternalParserFinalize);

            var (Defined, Accessed) = GetUsedOrders(context);
            foreach (var (Order, Location) in Defined)
            {
                if (!Accessed.ContainsAny(a => a == Order))
                {
                    context.ReportWarning(
                        WarningID.UnusedDefinedFootnote,
                        Location,
                        "Footnote `{0}` is not being used",
                        Location.RawReference);
                }
            }
        }

        private static readonly Type StorageKey = typeof(FootnoteParser);
        private static (List<(int Order, Reference Location)> Defined, List<int> Accessed) GetUsedOrders(ParsingContext context)
        {
            if (context.ParserStorage.TryGetValue(StorageKey, out object obj))
            {
                return ((List<(int, Reference)>, List<int>))obj;
            }
            else
            {
                var indexes = (new List<(int, Reference)>(), new List<int>());
                context.ParserStorage.Add(StorageKey, indexes);
                return indexes;
            }
        }
    }
}
