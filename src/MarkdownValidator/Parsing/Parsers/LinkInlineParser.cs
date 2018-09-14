/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
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
                string label = link.Label ?? (link.FirstChild as LiteralInline)?.Content.ToString();

                context.ParsingResult.AddReference(
                    new Reference(
                        label,
                        context.GetRelativePath(label),
                        link.Span,
                        link.Line,
                        link.IsImage));
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
                    context.ParsingResult.AddReference(
                        new Reference(
                            link.Url,
                            context.GetRelativePath(link.Url),
                            link.Span,
                            link.Line,
                            link.IsImage));
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
