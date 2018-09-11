/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Warnings;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal sealed class MarkdownDocumentParser : IParser
    {
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(MarkdownDocument), ParseDocument);
        }

        private void ParseDocument(ParsingContext context)
        {
            var document = context.Object as MarkdownDocument;
            context.SetWarningSource(WarningSource.InternalParser);

            if (document.LineCount > context.Configuration.Parsing.Warnings_HugeFile_LineCount)
            {
                context.ReportGeneralWarning(
                    WarningID.HugeFile,
                    "Markdown file is huge! Consider moving some content into other files.");
            }
        }
    }
}
