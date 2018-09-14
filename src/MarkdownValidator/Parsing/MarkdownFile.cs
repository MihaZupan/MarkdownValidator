﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class MarkdownFile
    {
        public readonly string FullPath;
        public readonly string RelativePath;
        public readonly string HtmlPath;

        public readonly Config Configuration;
        public ParsingResult ParsingResult { get; private set; }
        public readonly ParsingContext ParsingContext;

        internal MarkdownFile(string fullPath, string relativePath, string source, Config configuration)
        {
            FullPath = fullPath;
            RelativePath = relativePath;
            int extensionIndex = RelativePath.LastIndexOf('.');
            HtmlPath = extensionIndex == -1 ?
                RelativePath + ".html" :
                RelativePath.Substring(0, extensionIndex) + ".html";
            Configuration = configuration;
            ParsingContext = new ParsingContext(this);
            Parse(source);
        }

        internal ParsingResultGlobalDiff Update(string source)
        {
            var previous = ParsingResult;
            Parse(source);
            return new ParsingResultGlobalDiff(previous, ParsingResult);
        }
        internal ParsingResultGlobalDiff Update()
        {
            var previous = ParsingResult;
            ParsingResult = new ParsingResult(previous);
            Parse();
            return new ParsingResultGlobalDiff(previous, ParsingResult);
        }

        private void Parse(string source)
        {
            ParsingResult = new ParsingResult(source, Configuration.GetNewPipeline());
            Parse();
        }

        private void Parse()
        {
            Configuration.ParsingController.Parse(ParsingResult.SyntaxTree, this);
            foreach (var markdownObject in ParsingResult.SyntaxTree.Descendants())
            {
                Configuration.ParsingController.Parse(markdownObject, this);
            }

            ParsingResult.Finalize(ParsingContext);
        }
    }
}
