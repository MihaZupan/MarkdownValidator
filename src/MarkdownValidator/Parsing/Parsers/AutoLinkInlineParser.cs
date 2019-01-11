/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax.Inlines;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class AutoLinkInlineParser : IParser
    {
        public string Identifier => nameof(AutoLinkInlineParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(AutolinkInline), ParseAutoLinkInline);
        }

        private void ParseAutoLinkInline(ParsingContext context)
        {
            var link = context.Object as AutolinkInline;

            if (!link.IsEmail)
            {
                context.TryAddReference(link.Url, link.Span, link.Line, autoLink: true);
            }

            // else
            // Do we care about emails?
            // Do we add mailto and process it?
        }
    }
}
