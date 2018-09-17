/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Extensions.Footnotes;
using MihaZupan.MarkdownValidator.Warnings;
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

            var state = GetUsedOrders(context);
            foreach (Footnote footnote in footnoteGroup)
            {
                state.Accessed.Add(footnote.Order);
            }
        }

        private void ParseFootnoteLinkReferenceDefinition(ParsingContext context)
        {
            var linkDefinition = context.Object as FootnoteLinkReferenceDefinition;

            var state = GetUsedOrders(context);
            state.Defined.Add((
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

            var state = GetUsedOrders(context);
            foreach (var (Order, Location) in state.Defined)
            {
                if (!state.Accessed.ContainsAny(a => a == Order))
                {
                    context.ReportWarning(
                        WarningID.UnusedDefinedFootnote,
                        Location,
                        "Footnote `{0}` is not being used",
                        Location.RawReference);
                }
            }
        }

        private class FootnoteParserState
        {
            public readonly List<(int Order, Reference Location)> Defined = new List<(int Order, Reference Location)>();
            public readonly List<int> Accessed = new List<int>();
        }

        private static FootnoteParserState GetUsedOrders(ParsingContext context)
            => context.GetParserState<FootnoteParser, FootnoteParserState>(() => new FootnoteParserState());
    }
}
