/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing;
using System;
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

        public HashSet<string> DisabledParsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// If you wish to disable this warning, set this to a huge value
        /// <para>Defaults to 1500</para>
        /// </summary>
        public int Warnings_HugeFile_LineCount = 1500;

        internal void Initialize()
        {
            List<IParser> allowedParsers = new List<IParser>();
            foreach (var parser in Parsers)
            {
                if (!DisabledParsers.Contains(parser.Identifier))
                    allowedParsers.Add(parser);
            }
            Parsers = allowedParsers;

            CodeBlocks.Initialize();
        }
    }
}
