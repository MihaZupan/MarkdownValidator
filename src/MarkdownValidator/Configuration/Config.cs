/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.Parsing.ExternalUrls;
using MihaZupan.MarkdownValidator.WebIO;
using System;
using System.Collections.Generic;
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
            RootWorkingDirectory = rootWorkingDirectory;
        }

        /// <summary>
        /// The root directory for all files and directories referenced in your markdown files
        /// <para>It must be a parent to all files and directories added to the context</para>
        /// <para>It can be the root of the file system, if you want, but it must not be a subdirectory of any added files/directories</para>
        /// <para>Note that paths you provide don't have to actually exist on disk</para>
        /// <para>Setting it to an empty string is the same as <see cref="Environment.CurrentDirectory"/></para>
        /// </summary>
        [NonSerialized]
        public string RootWorkingDirectory;

        internal bool TryGetRelativePath(string fullPath, out string relativePath)
        {
            if (fullPath.StartsWith(RootWorkingDirectory, StringComparison.OrdinalIgnoreCase))
            {
                if (fullPath.Length == RootWorkingDirectory.Length)
                    relativePath = string.Empty;
                else relativePath = fullPath.Substring(RootWorkingDirectory.Length + 1);
                return true;
            }
            else
            {
                relativePath = null;
                return false;
            }
        }
        internal bool TryGetRelativePath(string sourceFile, string reference, out string relativePath)
        {
            string relative;

            if (reference.StartsWith('~'))
                reference = reference.Substring(1);

            if (reference.Contains(':'))
            {
                relativePath = reference;
                return true;
            }
            else if (reference.StartsWith('#'))
                relative = sourceFile + reference;
            else if (reference.StartsWith('/'))
                relative = reference.Substring(0, reference.Length - 1);
            else if (reference.OrdinalEquals("."))
                relative = Path.GetDirectoryName(sourceFile);
            else
                relative = Path.Combine(Path.GetDirectoryName(sourceFile), reference);

            return TryGetRelativePath(Path.GetFullPath(relative, RootWorkingDirectory), out relativePath);
        }
        /// <summary>
         /// Throws <see cref="PathNotInContextException"/>
         /// </summary>
        internal void EnsurePathIsValidForContext(string path, out string fullPath, out string relativePath)
        {
            fullPath = Path.GetFullPath(path, RootWorkingDirectory);
            if (!TryGetRelativePath(fullPath, out relativePath))
                throw new ArgumentException($"{path} is not a child path of the root working directory of the context");
        }

        /// <summary>
        /// A custom <see cref="MarkdownPipelineBuilder"/> provider
        /// <para>Each call to this method MUST return a NEW instance of <see cref="MarkdownPipelineBuilder"/></para>
        /// <para>Calls to this method may happen concurrently from multiple threads</para>	
        /// </summary>
        [NonSerialized]
        public Func<MarkdownPipelineBuilder> MarkdigPipeline = null;
        internal MarkdownPipeline GetNewPipeline()
        {
            var builder = MarkdigPipeline?.Invoke() ?? new MarkdownPipelineBuilder();

            // Add 'default' extensions
            return builder
                .UsePreciseSourceLocation()
                .UseAutoLinks()
                .UseFootnotes()
                .Build();
        }

        public ParsingConfig Parsing
        {
            get => _parsing;
            set => _parsing = value ?? throw new NullReferenceException(nameof(Parsing));
        }
        private ParsingConfig _parsing = new ParsingConfig();
        public WebIOConfig WebIO
        {
            get => _webIO;
            set => _webIO = value ?? throw new NullReferenceException(nameof(WebIO));
        }
        private WebIOConfig _webIO = new WebIOConfig();

        public HashSet<string> ExtensionAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> DisabledWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<int> DisabledWarningIDs = new HashSet<int>();

        public bool DisableExternalParsers;

        internal void Initialize()
        {
            if (RootWorkingDirectory is null)
                throw new ArgumentNullException(nameof(RootWorkingDirectory));

            RootWorkingDirectory = RootWorkingDirectory.Length == 0
                ? Environment.CurrentDirectory
                : Path.GetFullPath(RootWorkingDirectory).TrimEnd('\\', '/');

            if (!DisableExternalParsers)
            {
                ExtensionAssemblies.Add("MarkdownValidator.ExternalParsers.dll");
                foreach (var assembly in ExtensionAssemblies)
                    ExtensionLoader.Load(this, assembly, out _);
            }

            Parsing.Initialize();

            ParsingController = new ParsingController(this);
            WebIOController = new WebIOController(this);
            UrlProcessor = new UrlProcessor(this);
        }
        internal ParsingController ParsingController;
        internal WebIOController WebIOController;
        internal UrlProcessor UrlProcessor;
    }
}
