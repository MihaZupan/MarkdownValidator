/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class LinkInlineParser : IParser
    {
        public string Identifier => nameof(LinkInlineParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(LinkInline), ParseLinkInline);
        }

        private void ParseLinkInline(ParsingContext context)
        {
            var link = context.Object as LinkInline;
            context.SetWarningSource(WarningSource.InternalParser);

            bool canBeUrl = !(link.Parent is EmphasisInline);

            if (!link.UrlSpan.Value.IsEmpty &&
                (link.UrlSpan.Value.Start < link.Span.Start || link.UrlSpan.Value.End > link.Span.End))
            {
                string label;
                SourceSpan span;
                if (link.Label != null)
                {
                    label = link.Label;
                    span = link.LabelSpan.Value;

                    if (!span.IsEmpty && link.FirstChild is LiteralInline literal)
                    {
                        string content = literal.Content.ToString();
                        if (content.Equals(label, StringComparison.OrdinalIgnoreCase))
                        {
                            context.ReportWarning(
                                WarningIDs.SameLabelAndTargetReference,
                                string.Empty,
                                "You can use `[{0}]` instead of `[{0}][{1}]`",
                                content,
                                label);
                        }
                    }
                }
                else
                {
                    label = (link.FirstChild as LiteralInline).Content.ToString();
                    span = link.FirstChild.Span;
                }

                context.TryAddReference(label, link.LabelSpan.Value, link.Line, link.IsImage, canBeUrl);
            }
            else
            {
                if (link.Url.IsEffectivelyEmpty()) ReportEmptyReference(context);
                else
                {
                    context.TryAddReference(link.Url, link.UrlSpan.Value, link.Line, link.IsImage, canBeUrl, link.UrlSpan);

                    if (context.Source.OrdinalContains("( ") || context.Source.OrdinalContains(" )"))
                        ReportExcessSpace(context);
                }

                if (link.FirstChild == null) ReportEmptyLinkContent(context);
            }
        }

        private static void ReportExcessSpace(ParsingContext context)
        {
            context.ReportWarning(
                WarningIDs.ReferenceHasExcessWhitespace,
                string.Empty,
                "Excess space in reference");
        }
        private static void ReportEmptyReference(ParsingContext context)
        {
            context.ReportWarning(
                WarningIDs.EmptyReference,
                string.Empty,
                "Empty reference");
        }
        private static void ReportEmptyLinkContent(ParsingContext context)
        {
            context.ReportWarning(
                WarningIDs.EmptyLinkContent,
                string.Empty,
                "Empty link content");
        }
    }
}
