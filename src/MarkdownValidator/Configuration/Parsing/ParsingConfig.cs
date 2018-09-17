/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing;
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

        /// <summary>
        /// If you wish to disable this warning, set this to a huge value
        /// <para>Defaults to 1500</para>
        /// </summary>
        public int Warnings_HugeFile_LineCount = 1500;

        /// <summary>
        /// Defaults to true
        /// </summary>
        public bool Warnings_InvalidListNumberOrder_Enabled = true;

        internal void Initialize()
        {
            CodeBlocks.Initialize();
        }
    }
}
