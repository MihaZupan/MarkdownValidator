/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Warnings;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class LinkReferenceDefinitionParser : IParser
    {
        public string Identifier => nameof(LinkReferenceDefinitionParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(LinkReferenceDefinition), ParseLinkReferenceDefinition);
        }

        private void ParseLinkReferenceDefinition(ParsingContext context)
        {
            var referenceDefinition = context.Object as LinkReferenceDefinition;
            context.SetWarningSource(WarningSource.InternalParser);

            context.AddLocalReferenceDefinition(referenceDefinition);
        }
    }
}
