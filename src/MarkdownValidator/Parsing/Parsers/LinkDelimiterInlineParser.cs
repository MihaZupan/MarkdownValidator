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
    internal class LinkDelimiterInlineParser : IParser
    {
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(LinkDelimiterInline), ParseLinkDelimiter);
        }

        private void ParseLinkDelimiter(ParsingContext context)
        {
            var delimiter = context.Object as LinkDelimiterInline;

            if (delimiter.Span.Start <= context.ParsingResult.LastLinkDelimiterIndex)
                return;

            context.ParsingResult.LastLinkDelimiterIndex = delimiter.Span.End;

            var thirdChild = delimiter.FirstChild.NextSibling.NextSibling;

            string reference = context.Source.Substring(thirdChild.Span, 1);

            if (reference.EndsWith(' ') || reference.StartsWith(' '))
            {
                reference = reference.Trim();
            }

            context.ParsingResult.AddReference(
                new Reference(
                    reference,
                    context.GetRelativePath(reference),
                    delimiter.LastChild.Span,
                    delimiter.Line,
                    delimiter.IsImage));
        }
    }
}
