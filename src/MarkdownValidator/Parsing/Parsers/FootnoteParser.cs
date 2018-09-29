/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Extensions.Footnotes;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Warnings;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class FootnoteParser : IParser
    {
        public string Identifier => nameof(FootnoteParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(FootnoteGroup), ParseFootnoteGroup);
            context.Register(typeof(FootnoteLink), ParseFootnoteLink);
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

        private void ParseFootnoteLink(ParsingContext context)
        {
            var link = context.Object as FootnoteLink;

            if (link.IsBackLink)
                return;

            var footnote = link.Footnote;

            var paragraph = footnote[0] as ParagraphBlock;

            if (paragraph != null)
            {
                if (paragraph.Span.IsEmpty)
                {
                    context.ReportWarning(
                        WarningIDs.EmptyFootnoteDefinition,
                        footnote.LabelSpan,
                        string.Empty,
                        "Footnote definition for `{0}` is empty",
                        footnote.Order.ToString());
                    return;
                }
                else
                {
                    // Might contain other links
                }
            }
            else // Might not be a paragraph
            {

            }

            #warning ToDo
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
                if (state.Accessed.BinarySearch(Order) < 0)
                {
                    context.ReportWarning(
                        WarningIDs.UnusedDefinedFootnote,
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
