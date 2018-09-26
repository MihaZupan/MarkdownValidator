/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using MihaZupan.MarkdownValidator.Helpers;
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

        public bool DisableExternalParsers;
        public HashSet<string> ExtensionAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal void Initialize()
        {
            if (RootWorkingDirectory is null)
                throw new ArgumentNullException(nameof(RootWorkingDirectory));

            RootWorkingDirectory = RootWorkingDirectory.Length == 0
                ? Environment.CurrentDirectory
                : Path.GetFullPath(RootWorkingDirectory).TrimEnd('\\', '/');

            PathHelper = new PathHelper(RootWorkingDirectory);

            if (!DisableExternalParsers)
            {
                ExtensionAssemblies.Add("MarkdownValidator.ExternalParsers.dll");
                foreach (var assembly in ExtensionAssemblies)
                    ExtensionLoader.Load(this, assembly, out _);
            }

            ParsingController = new ParsingController(this);
            WebIOController = new WebIOController(this);
            UrlProcessor = new UrlProcessor(this);
        }
        internal PathHelper PathHelper;
        internal ParsingController ParsingController;
        internal WebIOController WebIOController;
        internal UrlProcessor UrlProcessor;
    }
}
