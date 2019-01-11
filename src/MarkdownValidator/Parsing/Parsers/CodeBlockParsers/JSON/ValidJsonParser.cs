/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Warnings;
using Newtonsoft.Json.Linq;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers.CodeBlockParsers.JSON
{
    internal class ValidJsonParser : JsonCodeBlockParser
    {
        public override string Identifier => nameof(ValidJsonParser);

        public override void ParseCodeBlock(CodeBlockInfo codeBlock, ParsingContext context)
        {
            JToken jToken = null;
            bool valid;
            try
            {
                jToken = JToken.Parse(codeBlock.Content);
                valid = true;
            }
            catch
            {
                valid = false;
            }

            if (!valid)
            {
                context.ReportWarning(
                    WarningIDs.InvalidJsonInJsonCodeBlock,
                    codeBlock.ContentSpan,
                    codeBlock.Content,
                    "JSON is not valid");
            }
        }
    }
}
