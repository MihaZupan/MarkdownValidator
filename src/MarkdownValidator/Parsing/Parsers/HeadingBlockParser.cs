/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class HeadingBlockParser : IParser
    {
        public string Identifier => nameof(HeadingBlockParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(HeadingBlock), ParseHeadingBlock);
        }

        private void ParseHeadingBlock(ParsingContext context)
        {
            var heading = context.Object as HeadingBlock;
            context.SetWarningSource(WarningSource.InternalParser);

            if (heading.HeaderChar == 0) return;
            var rawText = context.Source.Substring(heading.Span);
            if (heading.Inline.Span.IsEmpty)
            {
                context.ReportWarning(
                    WarningIDs.EmptyHeading,
                    rawText,
                    "Empty heading");
                return;
            }
            int offset = rawText.IndexOf('#', StringComparison.Ordinal);
            for (int i = offset + 1; i < rawText.Length; i++)
            {
                if (rawText[i] != '#')
                {
                    offset = i + 1;
                    break;
                }
            }
            string headerText = rawText.Substring(offset);

            var headerUrl = '#' + LinkHelper.UrilizeAsGfm(headerText);

            if (headerUrl.Length == 1 || !headerUrl.ContainsAny(c => c != '#' && c != '-'))
            {
                context.ReportWarning(
                    WarningIDs.EffectivelyEmptyHeading,
                    rawText,
                    "Heading is effectively empty");
                return;
            }

            if (headerUrl.EndsWith('-'))
            {
                context.ReportWarning(
                    WarningIDs.HeadingEndsWithWhitespace,
                    rawText,
                    "Heading `{0}` ends with a whitespace",
                    rawText);
            }

            context.ProcessRelativePath(headerUrl, out string relative);
            if (context.ParsingResult.HeadingDefinitions.ContainsAny(h =>
                h.GlobalReference.Equals(relative, StringComparison.OrdinalIgnoreCase),
                out ReferenceDefinition definition))
            {
                context.ReportWarning(
                    WarningIDs.DuplicateHeadingDefinition,
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
                        context.Object,
                        context.SourceFile));

                context.ProcessRelativeHtmlPath(headerUrl, out string relativeHtml);
                context.ParsingResult.HeadingDefinitions.Add(
                    new ReferenceDefinition(
                        rawText,
                        relativeHtml,
                        context.Object,
                        context.SourceFile));
            }
        }
    }
}
