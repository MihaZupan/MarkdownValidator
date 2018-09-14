/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MihaZupan.MarkdownValidator.Warnings;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class LinkInlineParser : IParser
    {
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(LinkInline), ParseLinkInline);
        }

        private void ParseLinkInline(ParsingContext context)
        {
            var link = context.Object as LinkInline;
            context.SetWarningSource(WarningSource.InternalParser);

            if (!link.UrlSpan.Value.IsEmpty &&
                (link.UrlSpan.Value.Start < link.Span.Start || link.UrlSpan.Value.End > link.Span.End))
            {
                string label;
                SourceSpan span;
                if (link.Label != null)
                {
                    label = link.Label;
                    span = link.LabelSpan.Value;
                }
                else
                {
                    label = (link.FirstChild as LiteralInline).Content.ToString();
                    span = link.FirstChild.Span;
                }

                if (context.TryGetRelativePath(label, out string relative))
                {
                    context.ParsingResult.AddReference(
                        new Reference(
                            label,
                            relative,
                            link.Span,
                            link.Line,
                            link.IsImage));
                }
                else
                {
                    context.ReportPathOutOfContext(label, link.Line, span);
                }
            }
            else
            {
                if (link.Url.Length == 0)
                {
                    context.ReportWarning(
                        WarningID.EmptyReference,
                        string.Empty,
                        "Empty reference");
                }
                else
                {
                    if (context.TryGetRelativePath(link.Url, out string relative))
                    {
                        context.ParsingResult.AddReference(
                            new Reference(
                                link.Url,
                                relative,
                                link.Span,
                                link.Line,
                                link.IsImage));
                    }
                    else
                    {
                        context.ReportPathOutOfContext(link.Url, link.Line, link.UrlSpan.Value);
                    }
                }

                if (link.FirstChild == null)
                {
                    context.ReportWarning(
                        WarningID.EmptyLinkContent,
                        string.Empty,
                        "Empty link content");
                }
            }
        }
    }
}
