/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
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

            bool canBeUrl = !(link.Parent is EmphasisInline);

            if (!link.UrlSpan.Value.IsEmpty && !link.UrlSpan.Value.Overlaps(link.Span))
            {
                string target;
                string name = null;
                SourceSpan span;
                if (link.Label != null)
                {
                    target = link.Label;
                    span = link.LabelSpan.Value;

                    if (!span.IsEmpty && link.FirstChild is LiteralInline literal)
                    {
                        name = literal.Content.ToString();
                        if (name.Equals(target, StringComparison.OrdinalIgnoreCase))
                        {
                            context.ReportWarning(
                                WarningIDs.SameLabelAndTargetReference,
                                string.Empty,
                                "You can use `[{0}]` instead of `[{0}][{1}]`",
                                name,
                                target);
                        }
                    }
                }
                else
                {
                    target = (link.FirstChild as LiteralInline).Content.ToString();
                    span = link.FirstChild.Span;
                }

                context.TryAddReference(target, link.LabelSpan.Value, link.Line, image: link.IsImage, cleanUrl: canBeUrl, namedReferenceLink: true, label: name);
            }
            else
            {
                string label = link.FirstChild == null ? null : context.Source.Substring(link.FirstChild.Span);

                if (link.Url.IsEffectivelyEmpty()) ReportEmptyReference(context);
                else
                {
                    context.TryAddReference(link.Url, link.UrlSpan.Value, link.Line, link.UrlSpan, link.IsImage, cleanUrl: canBeUrl, autoLink: link.IsAutoLink, label: label);

                    if (context.Source.OrdinalContains("( ") || context.Source.OrdinalContains(" )"))
                        ReportExcessSpace(context);
                }

                if (label == null) ReportEmptyLinkContent(context);
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
