/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax.Inlines;

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

            if (link.Span.Start <= context.ParsingResult.LastLinkInlineIndex)
                return;

            context.ParsingResult.LastLinkInlineIndex = link.Span.End;

            string value = link.Label ?? link.Url;

            context.ParsingResult.AddReference(
                new Reference(
                    value,
                    context.GetRelativePath(value),
                    link.Span,
                    link.Line,
                    link.IsImage));
        }
    }
}
