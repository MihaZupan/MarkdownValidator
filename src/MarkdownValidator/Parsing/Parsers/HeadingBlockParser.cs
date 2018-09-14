/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Warnings;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class HeadingBlockParser : IParser
    {
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(HeadingBlock), ParseHeadingBlock);
        }

        private void ParseHeadingBlock(ParsingContext context)
        {
            var heading = context.Object as HeadingBlock;
            context.SetWarningSource(WarningSource.InternalParser);

            var rawText = context.Source.Substring(heading.Span);
            if (heading.Inline.Span.IsEmpty)
            {
                context.ReportWarning(
                    WarningID.EmptyHeading,
                    rawText,
                    "Empty heading");
                return;
            }
            string headerText = rawText.Substring(heading.Inline.Span.Start - heading.Span.Start);

            var headerUrl = '#' + LinkHelper.UrilizeAsGfm(headerText);

            if (headerUrl.EndsWith('-'))
            {
                context.ReportWarning(
                    WarningID.HeadingEndsWithWhitespace,
                    rawText,
                    "Heading ends with a whitespace");
            }

            string relative = context.GetRelativePath(headerUrl);
            if (context.ParsingResult.HeadingDefinitions.ContainsAny(h =>
                h.GlobalReference.Equals(relative, StringComparison.OrdinalIgnoreCase),
                out ReferenceDefinition definition))
            {
                context.ReportWarning(
                    WarningID.DuplicateHeadingDefinition,
                    rawText,
                    "Duplicate heading definition `{0}` - already defined on line {1}",
                    headerText,
                    (definition.Line + 1).ToString());
            }
            else
            {
                context.ParsingResult.HeadingDefinitions.Add(
                    new ReferenceDefinition(
                        rawText,
                        relative,
                        heading.Span,
                        heading.Line,
                        context.SourceFile));

                context.ParsingResult.HeadingDefinitions.Add(
                    new ReferenceDefinition(
                        rawText,
                        context.GetRelativeHtmlPath(headerUrl),
                        heading.Span,
                        heading.Line,
                        context.SourceFile));
            }
        }
    }
}
