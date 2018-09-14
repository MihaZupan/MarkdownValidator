/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.WebIO;
using System;
using System.IO;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public class Config
    {
        /// <summary>
        /// Initializes a new <see cref="Config"/> based on the root working directory
        /// </summary>
        /// <param name="rootWorkingDirectory">The root directory for all files and directories referenced in your markdown files. See <see cref="RootWorkingDirectory"/></param>
        public Config(string rootWorkingDirectory)
        {
            if (rootWorkingDirectory is null)
                throw new ArgumentNullException(nameof(rootWorkingDirectory));
            if (rootWorkingDirectory == string.Empty)
                throw new ArgumentException(nameof(rootWorkingDirectory) + " must not be empty");

            RootWorkingDirectory = Path.GetFullPath(rootWorkingDirectory);
        }

        /// <summary>
        /// The root directory for all files and directories referenced in your markdown files
        /// <para>It must be a parent to all files and directories added to the context</para>
        /// <para>It can be the root of the file system, if you want, but it must not be a subdirectory of any added files/directories</para>
        /// <para>Note that paths you provide don't have to actually exist on disk</para>
        /// </summary>
        public string RootWorkingDirectory;
        internal string GetRelativePath(string fullPath)
        {
            if (fullPath.Equals(RootWorkingDirectory, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return fullPath.Substring(RootWorkingDirectory.Length + 1);
        }
        internal string GetRelativePath(string sourceFile, string reference)
        {
            string relative;

            if (reference.OrdinalContains(':'))
                return reference;
            else if (reference.StartsWith('#'))
                relative = sourceFile + reference;
            else if (reference.StartsWith('/'))
                return RootWorkingDirectory + reference;
            else if (reference.OrdinalEquals("."))
                relative = Path.GetDirectoryName(sourceFile);
            else
                relative = Path.Combine(Path.GetDirectoryName(sourceFile), reference);

            return GetRelativePath(Path.GetFullPath(relative, RootWorkingDirectory));
        }
        /// <summary>
         /// Throws <see cref="PathNotInContextException"/>
         /// </summary>
        internal void EnsurePathIsValidForContext(string path, out string fullPath, out string relativePath)
        {
            fullPath = Path.GetFullPath(path, RootWorkingDirectory);
            if (fullPath.StartsWith(RootWorkingDirectory, StringComparison.Ordinal))
            {
                relativePath = GetRelativePath(fullPath);
            }
            else
            {
                throw new ArgumentException($"{path} is not a child path of the root working directory of the context");
            }
        }

        /// <summary>
        /// A custom <see cref="Markdig.MarkdownPipeline"/> provider
        /// </summary>
        public IPipelineProvider PipelineProvider = null;
        internal MarkdownPipeline GetNewPipeline()
        {
            var builder = PipelineProvider?.GetNewPipeline() ?? new MarkdownPipelineBuilder();

            // Add default extensions
            return builder
                .UsePreciseSourceLocation()
                .UseAutoLinks()
                .UseFootnotes()
                .Build();
        }

        public ParsingConfig Parsing = new ParsingConfig();
        public WebIOConfig WebIO = new WebIOConfig();

        internal void Initialize()
        {
            ParsingController = new ParsingController(this);
            WebIOController = new WebIOController(this);
            LinkReferenceProcessor = new LinkReferenceProcessor(this);
        }
        internal ParsingController ParsingController;
        internal WebIOController WebIOController;
        internal LinkReferenceProcessor LinkReferenceProcessor;
    }
}
