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
    public sealed class MarkdownDocumentParser : IParser
    {
        public string Identifier => nameof(MarkdownDocumentParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(MarkdownDocument), ParseDocument);
        }

        private void ParseDocument(ParsingContext context)
        {
            var document = context.Object as MarkdownDocument;

            if (document.Count == 0)
            {
                context.ReportGeneralWarning(
                    ExternalWarningIDs.EmptyMarkdownFile,
                    string.Empty,
                    "Empty markdown file");
            }
            else if (document.LineCount >= context.Configuration.Parsing.Warnings_HugeFile_LineCount)
            {
                context.ReportGeneralWarning(
                    ExternalWarningIDs.HugeMarkdownFile,
                    $"{document.LineCount} >= {context.Configuration.Parsing.Warnings_HugeFile_LineCount}",
                    "Markdown file is huge! Consider moving some content into other files.");
            }
        }
    }
}
