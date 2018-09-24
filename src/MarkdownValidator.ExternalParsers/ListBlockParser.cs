/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Parsing;

namespace MihaZupan.MarkdownValidator.ExternalParsers
{
    public class ListBlockParser : IParser
    {
        public string Identifier => nameof(ListBlockParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(ListBlock), ParseListBlock);
        }

        private void ParseListBlock(ParsingContext context)
        {
            var list = context.Object as ListBlock;

            if (!list.IsOrdered)
                return;

            int expected = 0;
            foreach (ListItemBlock listItem in list)
            {
                if (listItem.Order != ++expected)
                {
                    context.ReportWarning(
                       ExternalWarningIDs.InvalidListNumberOrder,
                       listItem.Span,
                       $"{expected}-{listItem.Order}",
                       "Invalid list number order - expected {0}, not {1}",
                       expected.ToString(),
                       listItem.Order.ToString());
                    break;
                }
            }
        }
    }
}
