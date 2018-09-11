/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;

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

            var header = heading.Inline.FirstChild;

            var headerText = context.Source.Substring(header.Span);

            var headerUrl = '#' + LinkHelper.UrilizeAsGfm(headerText);

            context.ParsingResult.GlobalReferenceDefinitions.Add(
                new ReferenceDefinition(
                    '#' + headerText,
                    context.GetRelativePath(headerUrl),
                    heading.Span,
                    heading.Line,
                    context.SourceFile));
        }
    }
}
