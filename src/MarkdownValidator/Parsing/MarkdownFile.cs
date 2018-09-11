/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MihaZupan.MarkdownValidator.Configuration;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class MarkdownFile
    {
        public readonly string FullPath;
        public readonly string RelativePath;
        public readonly string RelativeLowercasePath;

        public readonly Config Configuration;
        public ParsingResult ParsingResult { get; private set; }

        private readonly ParsingController ParsingController;
        internal readonly ParsingContext ParsingContext;

        internal MarkdownFile(string fullPath, string relativePath, string source, MarkdownPipeline pipeline, ParsingController parsingController)
        {
            FullPath = fullPath;
            RelativePath = relativePath;
            RelativeLowercasePath = relativePath.ToLower();
            ParsingController = parsingController;
            Configuration = parsingController.Configuration;
            ParsingContext = new ParsingContext(this);
            Parse(source, pipeline);
        }

        internal ParsingResultGlobalDiff Update(string source, MarkdownPipeline pipeline)
        {
            var previous = ParsingResult;
            Parse(source, pipeline);
            return new ParsingResultGlobalDiff(previous, ParsingResult);
        }
        internal ParsingResultGlobalDiff Update()
        {
            var previous = ParsingResult;
            ParsingResult = new ParsingResult(previous);
            Parse();
            return new ParsingResultGlobalDiff(previous, ParsingResult);
        }

        private void Parse(string source, MarkdownPipeline pipeline)
        {
            ParsingResult = new ParsingResult(source, pipeline);
            Parse();
        }

        private void Parse()
        {
            Stack<ContainerBlock> blockContainers = new Stack<ContainerBlock>();
            Stack<ContainerInline> inlineContainers = new Stack<ContainerInline>();
            blockContainers.Push(ParsingResult.SyntaxTree);

            ParsingController.Parse(ParsingResult.SyntaxTree, this);

            while (blockContainers.Count > 0)
            {
                var container = blockContainers.Pop();
                foreach (var descendant in container.Descendants())
                {
                    if (descendant is ContainerBlock blockContainer)
                    {
                        blockContainers.Push(blockContainer);
                    }
                    else if (descendant is ContainerInline inlineContainer)
                    {
                        inlineContainers.Push(inlineContainer);
                    }
                    ParsingController.Parse(descendant, this);
                }
            }
            while (inlineContainers.Count > 0)
            {
                var container = inlineContainers.Pop();
                foreach (var descendant in container.Descendants())
                {
                    if (descendant is ContainerInline inlineContainer)
                    {
                        inlineContainers.Push(inlineContainer);
                    }
                    ParsingController.Parse(descendant, this);
                }
            }

            ParsingResult.Finalize(ParsingContext);
        }
    }
}
