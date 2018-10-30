/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing;
using Newtonsoft.Json;

namespace MihaZupan.MarkdownValidator.ExternalParsers
{
    public sealed class MarkdownDocumentParser : IParser
    {
        public string Identifier => nameof(MarkdownDocumentParser);

        public class CustomConfig : ICustomConfig
        {
            /// <summary>
            /// If you wish to disable this warning, set this to a huge value
            /// <para>Defaults to 1500</para>
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public int HugeFile_LineCount = 1500;

            [JsonProperty(Required = Required.Always)]
            public bool ReportEmptyFile = true;
        }

        private CustomConfig ParserConfig;
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(MarkdownDocument), ParseDocument);
            ParserConfig = context.ReadConfig<CustomConfig>(true);
        }

        private void ParseDocument(ParsingContext context)
        {
            var document = context.Object as MarkdownDocument;

            if (document.Count == 0)
            {
                if (ParserConfig.ReportEmptyFile)
                {
                    context.ReportGeneralWarning(
                        ExternalWarningIDs.EmptyMarkdownFile,
                        string.Empty,
                        "Empty markdown file");
                }
            }
            else if (document.LineCount >= ParserConfig.HugeFile_LineCount)
            {
                context.ReportGeneralWarning(
                    ExternalWarningIDs.HugeMarkdownFile,
                    $"{document.LineCount} >= {ParserConfig.HugeFile_LineCount}",
                    "Markdown file is huge! Consider moving some content into other files.");
            }
        }
    }
}
