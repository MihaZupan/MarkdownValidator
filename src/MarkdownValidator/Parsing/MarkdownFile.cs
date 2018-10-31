/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using System.Diagnostics;
using System.IO;

namespace MihaZupan.MarkdownValidator.Parsing
{
    [DebuggerDisplay("{RelativePath}")]
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
            HtmlPath = Path.ChangeExtension(RelativePath, ".html");
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
            ParsingResult = new ParsingResult(source, Configuration);
            Parse();
        }

        private void Parse()
        {
            var controller = Configuration.ParsingController;
            controller.Parse(ParsingResult.SyntaxTree, this);
            foreach (var markdownObject in ParsingResult.SyntaxTree.Descendants())
            {
                controller.Parse(markdownObject, this);
            }

            controller.Finalize(this);
            ParsingResult.Finalize(ParsingContext);
            StringBuilderCache.Local(); // Clear the StringBuilder buffer
        }
    }
}
