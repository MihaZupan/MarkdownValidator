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
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(LinkReferenceDefinition), ParseLinkReferenceDefinition);
        }

        private void ParseLinkReferenceDefinition(ParsingContext context)
        {
            var referenceDefinition = context.Object as LinkReferenceDefinition;
            context.SetWarningSource(WarningSource.InternalParser);

            if (context.TryGetRelativePath(referenceDefinition.Label, out string relativeLabel))
            {
                context.ParsingResult.LocalReferenceDefinitions.Add(
                    new ReferenceDefinition(
                        referenceDefinition.Label,
                        relativeLabel,
                        referenceDefinition.Span,
                        referenceDefinition.Line,
                        context.SourceFile));

                if (context.TryGetRelativePath(referenceDefinition.Url, out string relativeUrl))
                {
                    context.ParsingResult.AddReference(
                        new Reference(
                            referenceDefinition.Url,
                            relativeUrl,
                            referenceDefinition.Span,
                            referenceDefinition.Line));
                }
                else
                {
                    context.ReportPathOutOfContext(referenceDefinition.Url, referenceDefinition.Line, referenceDefinition.UrlSpan);
                }
            }
            else
            {
                context.ReportPathOutOfContext(referenceDefinition.Label, referenceDefinition.Line, referenceDefinition.LabelSpan);
            }
        }
    }
}
