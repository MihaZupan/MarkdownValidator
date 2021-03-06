﻿/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Parsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public class Config
    {
        [JsonIgnore]
        public static readonly Version Version = new Version(1, 3, 0);

        /// <summary>
        /// Initializes a new <see cref="Config"/> based on the root working directory
        /// </summary>
        /// <param name="rootWorkingDirectory">The root directory for all files and directories referenced in your markdown files. See <see cref="RootWorkingDirectory"/></param>
        public Config(string rootWorkingDirectory)
        {
            RootWorkingDirectory = rootWorkingDirectory;
        }

        /// <summary>
        /// Returns a new <see cref="Config"/>
        /// </summary>
        [JsonIgnore]
        public static Config Default => new Config("");

        /// <summary>
        /// The root directory for all files and directories referenced in your markdown files
        /// <para>It must be a parent to all files and directories added to the context</para>
        /// <para>It can be the root of the file system, if you want, but it must not be a subdirectory of any added files/directories</para>
        /// <para>Note that paths you provide don't have to actually exist on disk</para>
        /// <para>Setting it to an empty string is the same as <see cref="Environment.CurrentDirectory"/></para>
        /// </summary>
        [JsonProperty(Required = Required.DisallowNull)]
        public string RootWorkingDirectory = "";

        /// <summary>
        /// A custom <see cref="MarkdownPipelineBuilder"/> provider
        /// <para>Note that Markdig parsers should be thread-safe</para>
        /// </summary>
        [JsonIgnore]
        public Func<MarkdownPipelineBuilder> MarkdigPipelineProvider = null;
        internal MarkdownPipeline MarkdigPipeline;

        [JsonProperty(Required = Required.DisallowNull)]
        public ParsingConfig Parsing
        {
            get => _parsing;
            set => _parsing = value ?? throw new NullReferenceException(nameof(Parsing));
        }
        private ParsingConfig _parsing = new ParsingConfig();
        [JsonProperty(Required = Required.DisallowNull)]
        public WebIOConfig WebIO
        {
            get => _webIO;
            set => _webIO = value ?? throw new NullReferenceException(nameof(WebIO));
        }
        private WebIOConfig _webIO = new WebIOConfig();

        public bool DisableExternalParsers;
        [JsonProperty(Required = Required.DisallowNull)]
        public HashSet<string> ExtensionAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal void Initialize()
        {
            if (RootWorkingDirectory is null)
                throw new ArgumentNullException(nameof(RootWorkingDirectory));

            RootWorkingDirectory = PathHelper.GetDirectoryWithSeparator(RootWorkingDirectory);

            PathHelper = new PathHelper(RootWorkingDirectory);

            var pipelineBuilder = MarkdigPipelineProvider?.Invoke() ?? new MarkdownPipelineBuilder();
            // Add 'default' extensions
            MarkdigPipeline = pipelineBuilder
                .UsePreciseSourceLocation()
                .UseAutoLinks()
                .UseFootnotes()
                .Build();

            WebIO.Initialize(this);

            if (!DisableExternalParsers && ExtensionAssemblies != null)
            {
                foreach (var assembly in ExtensionAssemblies)
                    ExtensionLoader.TryLoad(this, assembly);
            }
            ParsingController = new ParsingController(this);
        }
        internal PathHelper PathHelper;
        internal ParsingController ParsingController;

        #region Custom Configurations

        private readonly object CustomConfigLock = new object();

        [JsonProperty(Required = Required.DisallowNull)]
        public Dictionary<string, JToken> CustomConfigurations = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);

        public TCustomConfig ReadConfig<TCustomConfig>(string identifier, bool writeIfDefault) where TCustomConfig : class, new()
        {
            lock (CustomConfigLock)
            {
                if (CustomConfigurations.TryGetValue(identifier, out JToken customConfigObj))
                {
                    try
                    {
                        return customConfigObj.ToObject<TCustomConfig>();
                    }
                    catch { }
                }

                var newConfig = new TCustomConfig();

                if (writeIfDefault)
                    CustomConfigurations[identifier] = JToken.FromObject(newConfig);

                return newConfig;
            }
        }
        public void WriteConfig(string identifier, object customConfig)
        {
            lock (CustomConfigLock)
            {
                CustomConfigurations[identifier] = JToken.FromObject(customConfig);
            }
        }

        #endregion Custom Configurations
    }
}
