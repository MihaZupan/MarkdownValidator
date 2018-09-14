/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.Parsing.Parsers;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public class ParsingConfig
    {
        public CodeBlocksConfig CodeBlocks = new CodeBlocksConfig();

        internal List<IParser> Parsers = new List<IParser>();
        public void AddParser(IParser parser)
        {
            Parsers.Add(parser);
        }

        public int Warnings_HugeFile_LineCount = 1500;
    }
}
